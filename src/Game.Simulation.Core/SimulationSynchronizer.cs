using Ecs.Core;

namespace Simulation.Core
{
    public interface ISimulationIngressCommands
    {
        void AddIngress(ISimulationIngressCommand command);
    }

    public interface ISimulationEgressCommands
    {
        void AddEgress(ISimulationEgressCommand command);
    }

    public sealed class SimulationSynchronizer : ISimulationIngressCommands, ISimulationEgressCommands
    {
        private readonly World _world;
        private readonly AppendOnlyList<ISimulationIngressCommand> _commandsIngress = new AppendOnlyList<ISimulationIngressCommand>(16);
        private readonly AppendOnlyList<ISimulationEgressCommand> _commandsEgress = new AppendOnlyList<ISimulationEgressCommand>(16);

        private object _lock = new object();

        public SimulationSynchronizer(World world)
        {
            this._world = world;
        }

        public void Sync()
        {
            lock (this._lock)
            {
                for (int i = 0; i < this._commandsIngress.Count; i++)
                {
                    var command = this._commandsIngress.Items[i];

                    if (command.CanExecute(this._world))
                    {
                        command.Execute(this._world, (ISimulationEgressCommands)this);
                    }
                }

                this._commandsIngress.Clear();
            }
        }

        public void AddIngress(ISimulationIngressCommand command)
        {
            lock (this._lock)
            {
                this._commandsIngress.Add(command);
            }
        }

        public void AddEgress(ISimulationEgressCommand command)
        {
            throw new System.NotImplementedException();
        }
    }
}
