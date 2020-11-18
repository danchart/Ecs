using Ecs.Core;

namespace Simulation.Core
{
    public interface ISimulationEgressCommand
    {
        bool CanExecute(World world);

        void Execute(World world);
    }
}
