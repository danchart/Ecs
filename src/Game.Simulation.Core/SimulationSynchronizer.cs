using Ecs.Core;

namespace Simulation.Core
{
    public interface ISimulationCommands
    {
        void Add(ISimulationCommand command);
    }

    public sealed class SimulationSynchronizer : ISimulationCommands
    {
        private readonly World _world;
        private readonly AppendOnlyList<ISimulationCommand> _commandsIngress = new AppendOnlyList<ISimulationCommand>(16);

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

        public void Add(ISimulationCommand command)
        {
            lock (this._lock)
            {
                this._commandsIngress.Add(command);
            }
        }
    }
}
