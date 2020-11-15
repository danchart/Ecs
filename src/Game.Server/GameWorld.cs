using Common.Core;
using Ecs.Core;
using Game.Networking;
using Game.Simulation.Core;
using Game.Simulation.Server;
using Simulation.Core;
using System;
using System.Threading;

namespace Game.Server
{
    public sealed class GameWorld
    {
        private bool _isStopped;

        public readonly WorldType WorldType;
        public readonly WorldInstanceId InstanceId;

        private readonly World _world;
        private readonly Systems _systems;
        private readonly ServerSimulation<InputComponent> _simulation;

        private readonly IPhysicsWorld _physicsWorld;

        private readonly IEntityGridMap _entityGridMap;

        private readonly WorldPlayers _players;

        private readonly IWorldReplicationManager _replicationManager;

        private readonly ServerChannelOutgoing _channelManager;

        private readonly ILogger _logger;
        private readonly float _fixedTick;

        public GameWorld(
            WorldType worldType,
            WorldInstanceId id, 
            ILogger logger,
            IServerConfig config,
            ServerChannelOutgoing channelManager,
            IGameWorldLoader gameWorldLoader)
        {
            this.WorldType = worldType;
            this.InstanceId = id;
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._isStopped = false;
            this._world = new World(config.Ecs);

            this._fixedTick = config.Simulation.FixedTick;

            this._channelManager = channelManager ?? throw new ArgumentNullException(nameof(channelManager));

            this._entityGridMap = new EntityGridMap(config.Replication.GridSize);

            this._players = new WorldPlayers(
                config.Replication,
                config.World,
                config.PlayerConnection.Capacity.InitialConnectionsCapacity);

            this._replicationManager = new WorldReplicationManager(config.Replication, this._players, this._entityGridMap);

            this._physicsWorld = new VolatilePhysicsWorld();

            this._systems =
                new Systems(this._world)
                .Add(new GatherReplicatedDataSystem())
                .Inject(new ReplicationDataBroker(config.Replication.Capacity, this._replicationManager))
                .Inject(this._entityGridMap);

            this._simulation = new ServerSimulation<InputComponent>(
                config.Simulation,
                this._world,
                this._systems);

            this._simulation.Create();

            gameWorldLoader.LoadWorld(this.WorldType, this._world, this._physicsWorld);
        }

        public void Run()
        {
            int tickMillieconds = (int) (1000 * this._fixedTick);

            var state = new FixedUpdateState
            {
                WaitHandle = new AutoResetEvent(initialState: false),

                FrameIndex = FrameIndex.New(),
            };

            // TODO: Timer is limited to system clock resolution. The max (worst) seems to be 15.625 ms which 
            // is maybe sufficient (64 ticks/s). See ClockRes SysInternal tool.
            //
            // In the future look into using Windows multi-media (winmm.dll) timers:
            //  https://www.codeproject.com/Articles/17474/Timer-surprises-and-how-to-avoid-them
            using (var stateTimer = new Timer(
                callback: Update,
                state: state,
                dueTime: 0,
                period: tickMillieconds))
            {
                _logger.Info($"World started: Id={this.InstanceId:x8}, period={tickMillieconds}ms");

                state.WaitHandle.WaitOne();
            }

            _logger.Info($"World stopped: Id={this.InstanceId:x8}");
        }

        private void Update(object obj)
        {
            // This callback can be invoked concurrently from the timer, so we must lock. Because we are simply 
            // running the simulation on the next frame & tick we can simply lock and let the next callback handle 
            // the next simulation tick.

            lock (obj)
            {
                var state = (FixedUpdateState)obj;

                // Run simulation tick
                this._simulation.FixedUpdate(this._fixedTick);

                // Update clients
                this._channelManager.ReplicateToClients(state.FrameIndex, this._players);

                // Increment frame index
                state.FrameIndex = state.FrameIndex.GetNext();

                if (this._isStopped)
                {
                    state.WaitHandle.Set();
                }
            }
        }

        public void Stop()
        {
            this._isStopped = true;
        }

        public void Connect(PlayerConnectionRef connectionRef)
        {
            ref var connection = ref connectionRef.Unref();

            if (this._players.Contains(connection.PlayerId))
            {
                this._logger.Warning($"Tried to connect player already in world: worldId={this.InstanceId}, playerId={connection.PlayerId}");

                return;
            }

            connection.WorldId = this.InstanceId;
            connection.LastAcknowledgedSimulationFrame = FrameIndex.Nil;
            connection.LastInputFrame = FrameIndex.Nil;

            // TODO: Player will need more sophisticated construction, e.g. components, game objects
            var playerEntity = this._simulation.World.NewEntity();
            playerEntity.GetComponent<TransformComponent>();
            playerEntity.GetComponent<MovementComponent>();

            ref var playerComponent = ref playerEntity.GetComponent<PlayerComponent>();
            playerComponent.Id = connection.PlayerId;

            this._players.Add(
                in connectionRef,
                playerEntity);
        }

        public void Disconnect(PlayerConnectionRef connectionRef)
        {
            ref var connection = ref connectionRef.Unref();

            if (!this._players.Contains(connection.PlayerId))
            {
                this._logger.Warning($"Tried to disconnect player not in world: worldId={this.InstanceId}, playerId={connection.PlayerId}");

                return;
            }

            var entity = this._players[connection.PlayerId].Entity;
            this._players.Remove(connection.PlayerId);
            entity.Free();
        }

        private class FixedUpdateState
        {
            public AutoResetEvent WaitHandle;

            public FrameIndex FrameIndex;
        }
    }
}
