using Common.Core;
using Ecs.Core;
using Game.Networking;
using Game.Simulation.Core;
using Game.Simulation.Server;
using Simulation.Core;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Game.Server
{
    public sealed class GameWorld
    {
        private bool _isLoaded;
        private FrameIndex _frameIndex;

        private bool _isStopped;

        public readonly WorldInstanceId Id;

        private readonly World _world;
        private readonly Systems _systems;
        private readonly ServerSimulation<InputComponent> _simulation;

        private readonly IPhysicsWorld _physicsWorld;

        private readonly WorldPlayers _players;

        private readonly IWorldReplicationManager _replicationManager;

        private readonly ServerChannelOutgoing _channelManager;

        private readonly ILogger _logger;
        private readonly float _fixedTick;

        public GameWorld(
            WorldInstanceId id, 
            ILogger logger,
            IServerConfig config,
            ServerChannelOutgoing channelManager)
        {
            this._isLoaded = false;
            this._frameIndex = FrameIndex.New();
            this.Id = id;
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._isStopped = false;
            this._world = new World(config.Ecs);

            this._fixedTick = config.Simulation.FixedTick;

            this._channelManager = channelManager ?? throw new ArgumentNullException(nameof(channelManager));

            this._players = new WorldPlayers(
                config.Replication,
                config.World,
                config.PlayerConnection.Capacity.InitialConnectionsCapacity);

            this._replicationManager = new WorldReplicationManager(config.Replication, this._players);

            this._physicsWorld = new VolatilePhysicsWorld();

            this._systems =
                new Systems(this._world)
                .Add(new GatherReplicatedDataSystem())
                .Inject(
                    new ReplicationDataBroker(
                        config.Replication.Capacity, 
                        this._replicationManager));

            this._simulation = new ServerSimulation<InputComponent>(
                config.Simulation,
                this._world,
                this._systems);

            this._simulation.Create();
        }

        public void Load(IGameWorldLoader loader)
        {
            _isLoaded = loader.LoadWorld(this._world, this._physicsWorld);
        }

        public void Run()
        {
            if (!this._isLoaded)
            {
                throw new InvalidOperationException("Cannot run a world until it has been loaded.");
            }

            int tickMillieconds = (int) (1000 * this._fixedTick);

            var autoEvent = new AutoResetEvent(initialState: false);

            // TODO: Timer is limited to system clock resolution. The max (worst) seems to be 15.625 ms which 
            // is maybe sufficient (64 ticks/s). See ClockRes SysInternal tool.
            using (var stateTimer = new Timer(
                callback: Update,
                state: autoEvent,
                dueTime: 0,
                period: tickMillieconds))
            {
                _logger.Info($"World started: Id={this.Id:x8}, period={tickMillieconds}ms");

                autoEvent.WaitOne();
            }

            _logger.Info($"World stopped: Id={this.Id:x8}");
        }

        private void Update(object stateInfo)
        {
            AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;

            // Run simulation tick
            this._simulation.FixedUpdate(this._fixedTick);

            // Update clients
            this._channelManager.ReplicateToClients(this._frameIndex, this._players);

            // Increment frame index
            this._frameIndex = this._frameIndex.GetNext();

            if (this._isStopped)
            {
                autoEvent.Set();
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
                this._logger.Warning($"Tried to connect player already in world: worldId={this.Id}, playerId={connection.PlayerId}");

                return;
            }

            connection.WorldId = this.Id;
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
                this._logger.Warning($"Tried to disconnect player not in world: worldId={this.Id}, playerId={connection.PlayerId}");

                return;
            }

            var entity = this._players[connection.PlayerId].Entity;
            this._players.Remove(connection.PlayerId);
            entity.Free();
        }
    }
}
