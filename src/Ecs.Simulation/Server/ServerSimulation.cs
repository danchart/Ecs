using Ecs.Core;

namespace Ecs.Simulation.Server
{
    public class ServerSimulation<TInput>
        where TInput : unmanaged
    {
        internal readonly World World;

        internal readonly Systems _fixedUpdate;

        private readonly SimulationConfig _config;

        private float _lastFixedTime = 0;

        public ServerSimulation(
            SimulationConfig config,
            World world,
            Systems fixedUpdate)
        {
            _config = config;
            World = world;
            _fixedUpdate = fixedUpdate;
        }

        public void Create()
        {
            _fixedUpdate.Create();
        }

        public void FixedUpdate(float deltaTime)
        {
            _fixedUpdate.Run(deltaTime);

            // Advance to next fixed time.
            _lastFixedTime += deltaTime;
        }
    }
}
