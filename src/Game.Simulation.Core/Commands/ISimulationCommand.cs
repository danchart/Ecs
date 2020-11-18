using Ecs.Core;

namespace Simulation.Core
{
    public interface ISimulationCommand
    {
        bool CanExecute(World world);

        void Execute(World world);
    }
}
