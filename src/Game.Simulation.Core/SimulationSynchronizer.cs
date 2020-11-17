using Common.Core;
using System.Threading;

namespace Simulation.Core
{
    public interface ISimulationSynchronizer
    {
        void Sync();

        void Lock();
        void Unlock();
    }

    public sealed class SimulationSynchronizer : ISimulationSynchronizer
    {
        private int _lockCount = 0;

        public SimulationSynchronizer()
        {
        }

        public void Lock()
        {
            Interlocked.Increment(ref _lockCount);
        }

        public void Sync()
        {
            throw new System.NotImplementedException();
        }

        public void Unlock()
        {
            if (Interlocked.Decrement(ref _lockCount) == 0)
            {
                // Add pending changes.
            }
        }

        public void AddPlayer(PlayerId id)
        {

        }
    }
}
