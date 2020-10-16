using Ecs.Core;
using Game.Simulation.Core;

namespace Game.Simulation.Client
{
    public class ClientSimulation<TInput>
        where TInput : unmanaged
    {
        internal readonly World World;

        internal readonly Systems _update;
        internal readonly Systems _fixedUpdate;

        private readonly SnapShots<TInput> _snapShots;

        private readonly SimulationConfig _config;

        private float _lastFixedTime = 0;
        private float _time = 0;

        public ClientSimulation(
            SimulationConfig config,
            World world,
            Systems update,
            Systems fixedUpdate)
        {
            _config = config;
            World = world;
            _update = update;
            _fixedUpdate = fixedUpdate;

            _snapShots = new SnapShots<TInput>(config.SnapShotCount);
        }

        public void Create()
        {
            _update.Create();
            _fixedUpdate.Create();
        }

        public void Rewind(int fixedFrameCount)
        {
            // Rewind world state to snapshot
            _snapShots.Seek(-fixedFrameCount);
            _snapShots.Current()
                .WorldState
                .CopyStateTo(ref World.State);

            // Rewind clocks to snapshot time (which is by definition always the fixed tick).
            _time = _snapShots.Current().Time;
            _lastFixedTime = _time;
        }

        public void PlayForward(int fixedFrameCount)
        {
            // Clone the snapshot inputs
            var clonedInputs = new AppendOnlyList<AppendOnlyList<SnapShot<TInput>.ClientInputFrame>>(fixedFrameCount);

            for (int i = 0; i < fixedFrameCount; i++)
            {
                SnapShot<TInput> replaySnapShot = _snapShots.Peek(i);

                clonedInputs.Add(new AppendOnlyList<SnapShot<TInput>.ClientInputFrame>(replaySnapShot.ClientInputs.Count));

                replaySnapShot.MoveInputsTo(ref clonedInputs.Items[i]);
            }

            for (int i = 0; i < fixedFrameCount; i++)
            {
                // Replay inputs and simulate.

                float tickRemaining = _config.FixedTick;

                var query = (EntityQuery<TInput>)World.GetEntityQuery<EntityQuery<TInput>>();
                ref var input = ref query.GetSingleton();

                // Apply all inputs before this fixed update
                for (int j = 0; j < clonedInputs.Items[i].Count; j++)
                {
                    input = clonedInputs.Items[i].Items[j].Input;

                    float deltaTime = clonedInputs.Items[i].Items[j].Time - _time;

                    //Debug.WriteLine($"{i}:{_time:N1}: LDown={input.isLeftDown}, LUp={input.isLeftUp}, RDown={input.isRightDown}, RUp={input.isRightUp}");

                    Update(deltaTime);

                    tickRemaining -= deltaTime;

                    //Debug.WriteLine($"Position={position.x}");
                }

                // Run this snapshots fixed update.
                FixedUpdate(_config.FixedTick);
            }
        }

        public void FixedUpdate(float deltaTime)
        {
            _fixedUpdate.Run(deltaTime);

            //Debug.WriteLine($"Saving snapshot # - T:{_lastFixedTime}");

            _snapShots.NextSnapshot(_lastFixedTime, World);

            // Advance to next fixed time.
            _lastFixedTime += deltaTime;
        }

        /// <summary>
        /// Update loop.
        /// </summary>
        public void Update(float deltaTime)
        {
            _time += deltaTime;

            // Capture input to the current snapshot
            var query = (EntityQuery<TInput>)World.GetEntityQuery<EntityQuery<TInput>>();
            ref readonly var input = ref query.GetSingletonComponentReadonly();

            _snapShots.Current()
                .ClientInputs
                .Add(new SnapShot<TInput>.ClientInputFrame
                {
                    Time = _time,
                    Input = input,
                });

            _update.Run(deltaTime);
        }
    }
}

