using Ecs.Core;
using Game.Simulation.Core;
using Game.Simulation.Server;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Game.Server
{
    public class GameWorld
    {
        //
        // Configurations
        //

        private readonly EcsConfig _ecsConfig = EcsConfig.Default;
        private readonly SimulationConfig _simulationConfig = SimulationConfig.Default;
        private readonly ReplicationConfig _replicationConfig = ReplicationConfig.Default;
        private readonly PlayerConnectionConfig _playerConnectionConfig = PlayerConnectionConfig.Default;

        private readonly World _world;
        private readonly Systems _systems;
        private readonly ServerSimulation<PlayerInputComponent> _simulation;

        private readonly IReplicationManager _replicationManager;

        private readonly PlayerConnections _playerConnections;

        private readonly ILogger _logger;

        private readonly uint _id;

        private bool _isStopped;

        public GameWorld(uint id, ILogger logger)
        {

            //_simulationConfig.FixedTick = 0.5f;


            this._id = id;
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._isStopped = false;
            this._world = new World(this._ecsConfig);

            _playerConnections = new PlayerConnections(this._replicationConfig, _playerConnectionConfig);
            _replicationManager = new ReplicationManager(_replicationConfig, _playerConnections);

            this._systems =
                new Systems(this._world)
                .Add(new GatherReplicatedDataSystem())
                .Inject(
                    new ReplicationDataBroker(
                        this._replicationConfig.Capacity, 
                        this._replicationManager));

            this._simulation = new ServerSimulation<PlayerInputComponent>(
                this._simulationConfig,
                this._world,
                this._systems);

            this._simulation.Create();
        }

        public uint Id => this._id;

        public void Run()
        {
            int tickMillieconds = (int) (1000 * _simulationConfig.FixedTick);

            var autoEvent = new AutoResetEvent(initialState: false);

            using (var stateTimer = new Timer(
                callback: Execute,
                state: autoEvent,
                dueTime: 0,
                period: tickMillieconds))
            {
                _logger.Info($"World started: Id={this._id:x8}, period={tickMillieconds}ms");

                autoEvent.WaitOne();
            }

            _logger.Info($"World stopped: Id={this._id:x8}");
        }

        private void Execute(object stateInfo)
        {
            AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;

            this._simulation.FixedUpdate(_simulationConfig.FixedTick);

            if (this._isStopped)
            {
                autoEvent.Set();
            }
        }

        public void Stop()
        {
            this._isStopped = true;
        }
    }
}
