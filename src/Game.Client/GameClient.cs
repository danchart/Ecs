using Common.Core;
using Ecs.Core;
using Game.Networking;
using Game.Server.Contracts;
using Game.Simulation.Client;
using Game.Simulation.Core;
using Simulation.Core;
using System;
using System.Threading;

namespace Game.Client
{
    public class GameClient
    {
        private readonly VolatilePhysicsWorld _physicsWorld;

        private readonly World _world;
        private readonly Systems _systems;
        private readonly Systems _fixedSystems;
        private readonly ClientSimulation<InputComponent> _simulation;
        private readonly PacketJitterBuffer _jitterBuffer;
        private readonly NetworkEntityMap _entityServerToClientMap;

        private readonly GameServerClient _server;

        private readonly ILogger _logger;

        public GameClient(
            ILogger logger,
            IJsonSerializer jsonSerializer,
            IClientConfig config)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this._jitterBuffer = new PacketJitterBuffer(this._logger, config.Jitter.Capacity);
            this._entityServerToClientMap = new NetworkEntityMap(config.Ecs.InitialEntityPoolCapacity);

            this._world = new World(config.Ecs);
            this._physicsWorld = new VolatilePhysicsWorld(historyLength: config.Simulation.SnapShotCount);

            this._systems = new Systems(this._world);
            this._fixedSystems =
                new Systems(this._world)
                .Add(new ClientEntityReplicationSystem())
                .Add(new PhysicsSystem())
                .Inject(this._world)
                .Inject(this._physicsWorld)
                .Inject(this._jitterBuffer)
                .Inject(this._entityServerToClientMap);

            this._simulation = new ClientSimulation<InputComponent>(
                config.Simulation,
                this._world,
                this._systems,
                this._fixedSystems);

            this._simulation.Create();

            this._server = new GameServerClient(
                this._logger, 
                jsonSerializer, 
                this._jitterBuffer, 
                config.NetworkTransport);
        }

        public void Start(string connectionServerEndPoint)
        {
            this._server.Start(connectionServerEndPoint);
        }

        public void Stop()
        {
            this._server.Stop();
        }

        public void FixedUpdate(float deltaTime, InputComponent input)
        {
            this._simulation.FixedUpdate(deltaTime);
        }

        public void Update(float deltaTime)
        {
            this._simulation.Update(deltaTime);
        }
    }
}
