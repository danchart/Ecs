using Ecs.Core;

namespace Simulation.Core
{
    public sealed class SimulationSynchronizer
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
                        command.Execute(this._world);
                    }
                }

                this._commandsIngress.Clear();
            }
        }

        public void Add(ISimulationIngressCommand command)
        {
            lock (this._lock)
            {
                this._commandsIngress.Add(command);
            }
        }
    }
}
