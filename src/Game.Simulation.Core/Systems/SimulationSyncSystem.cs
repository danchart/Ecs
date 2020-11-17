using Ecs.Core;

namespace Simulation.Core
{
    /// <summary>
    /// Synchronizes the simulation state with any external changes, e.g. entity spawn/despawn.
    /// </summary>
    public class SimulationSyncSystem : SystemBase
    {
        private ISimulationSynchronizer _synchronizer = null;

        public override void OnUpdate(float deltaTime)
        {
            this._synchronizer.Lock();

            try
            {
                this._synchronizer.Sync();
            }
            finally
            {
                this._synchronizer.Unlock();
            }
        }
    }
}
