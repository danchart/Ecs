using Common.Core;
using Ecs.Core;
using Game.Simulation.Client;
using Game.Simulation.Core;
using Simulation.Core;
using System;

namespace Game.Client
{
    public class GameClient
    {
        private readonly VolatilePhysicsWorld _physicsWorld;

        private readonly World _world;
        private readonly Systems _systems;
        private readonly Systems _fixedSystems;
        private readonly ClientSimulation<InputComponent> _simulation;

        private readonly ILogger _logger;

        public GameClient(ILogger logger, IClientConfig config)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this._world = new World(config.Ecs);
            this._physicsWorld = new VolatilePhysicsWorld(historyLength: config.Simulation.SnapShotCount);

            this._systems = new Systems(this._world);
            this._fixedSystems =
                new Systems(this._world)
                .Add(new PhysicsSystem())
                .Inject(this._physicsWorld);

            this._simulation = new ClientSimulation<InputComponent>(
                config.Simulation,
                this._world,
                this._systems,
                this._fixedSystems);
        }

        public void FixedUpdate(float deltaTime)
        {
            this._simulation.FixedUpdate(deltaTime);
        }

        public void Update(float deltaTime)
        {
            this._simulation.Update(deltaTime);
        }
    }
}
