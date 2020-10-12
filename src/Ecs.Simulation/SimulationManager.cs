using Ecs.Core;
using System.Diagnostics;

namespace Ecs.Simulation
{
    public class SimulationManager<TInput>
        where TInput : unmanaged
    {
        public const int SnapShotCount = 10; // in fixed update frames.

        internal readonly float FixedTick;

        internal readonly World World;

        internal readonly Systems Update;
        internal readonly Systems FixedUpdate;

        private float _lastFixedTime = 0;
        private float _time = 0;

        private readonly SnapShots<TInput> _snapShots;

        private const float TickEpsilon = 0.000001f;

        public SimulationManager(
            float fixedTick,
            World world,
            Systems update,
            Systems fixedUpdate)
        {
            FixedTick = fixedTick;
            World = world;
            Update = update;
            FixedUpdate = fixedUpdate;

            _snapShots = new SnapShots<TInput>(SnapShotCount);
        }

        public void Create()
        {
            Update.Create();
            FixedUpdate.Create();
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

                float tickRemaining = FixedTick;

                var query = (EntityQuery<TInput>)World.GetEntityQuery<EntityQuery<TInput>>();
                ref var input = ref query.GetSingleton();

                // Apply all inputs for this fixed update
                for (int j = 0; j < clonedInputs.Items[i].Count; j++)
                {
                    input = clonedInputs.Items[i].Items[j].Input;

                    float deltaTime = clonedInputs.Items[i].Items[j].Time - _time;

                    //Debug.WriteLine($"{i}:{_time:N1}: LDown={input.isLeftDown}, LUp={input.isLeftUp}, RDown={input.isRightDown}, RUp={input.isRightUp}");

                    Run(deltaTime);

                    tickRemaining -= deltaTime;

                    //Debug.WriteLine($"Position={position.x}");
                }

                if (tickRemaining > -TickEpsilon)
                {
                    Run(tickRemaining);
                }
            }
        }

        /// <summary>
        /// Simulated update loop.
        /// </summary>
        public void Run(float deltaTime)
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

            while (_time - _lastFixedTime >= FixedTick)
            {
                FixedUpdate.Run(FixedTick);

                Debug.WriteLine($"Saving snapshot # - T:{_lastFixedTime}");

                _snapShots.NextSnapshot(_lastFixedTime, World);

                // Advance to next fixed time.
                _lastFixedTime += FixedTick;
            }

            Update.Run(deltaTime);
        }
    }
}

