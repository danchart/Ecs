using Ecs.Core;
using Game.Simulation.Core;

namespace Game.Simulation.Server
{
    public class ServerSimulation<TInput>
        where TInput : unmanaged
    {
        internal readonly World World;

        // Server simulation uses only fixed updates (for now?)
        internal readonly Systems _fixedUpdate;

        private readonly SimulationConfig _config;

        private float _lastFixedTime;

        public ServerSimulation(
            SimulationConfig config,
            World world,
            Systems fixedUpdate)
        {
            this._config = config;
            this.World = world;
            this._fixedUpdate = fixedUpdate;
            this._lastFixedTime = 0;
        }

        public void Create()
        {
            this._fixedUpdate.Create();
        }

        public void FixedUpdate(float deltaTime)
        {
            this._fixedUpdate.Run(deltaTime);

            // Advance to next fixed time.
            this._lastFixedTime += deltaTime;
        }
    }
}
