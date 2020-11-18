using Ecs.Core;

namespace Simulation.Core
{
    public interface ISimulationIngressCommand
    {
        bool CanExecute(World world);

        void Execute(World world);
    }
}
