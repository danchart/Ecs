using Ecs.Core;
using Game.Simulation.Core;
using Game.Simulation.Server;
using System.Diagnostics;
using System.Threading;

namespace Game.Server
{
    public class GameWorld
    {
        private readonly EcsConfig _ecsConfig = EcsConfig.Default;
        private readonly SimulationConfig _simulationConfig = SimulationConfig.Default;

        private readonly World _world;
        private readonly Systems _systems;
        private readonly ServerSimulation<PlayerInputComponent> _simulation;

        private bool _isStopped;

        public GameWorld()
        {
            this._isStopped = false;
            this._world = new World(this._ecsConfig);

            this._systems =
                new Systems(this._world)
                .Add(new GatherReplicatedDataSystem());

            this._simulation = new ServerSimulation<PlayerInputComponent>(
                this._simulationConfig,
                this._world,
                this._systems);

            this._simulation.Create();
        }

        public void Run()
        {
            const int deltaTime = (int) (1000 * _simulationConfig.FixedTick);

            var stopWatch = new Stopwatch();

            stopWatch.Start();

            while (!this._isStopped)
            {
                while(stopWatch.ElapsedMilliseconds )

                this._simulation.FixedUpdate(_simulationConfig.FixedTick);

                Thread.Sleep((int) (1000 * _simulationConfig.FixedTick));
            }

            
        }

        public void Stop()
        {

        }
    }
}
