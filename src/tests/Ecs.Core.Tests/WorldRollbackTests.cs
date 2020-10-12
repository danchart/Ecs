using System;
using System.Diagnostics;
using Xunit;

namespace Ecs.Core.Tests
{
    public class WorldRollbackTests
    {

        [Fact]
        public void Rollback()
        {
            const float deltaTime = 1f / 10f; // Running 10 FPS
            const float fixedDeltaTime = 1f / 5f; // Fixed tick at 5 FPS

            //const float deltaTime = 1f / 60f; // Running 10 FPS
            //const float fixedDeltaTime = 1f / 30f; // Fixed tick at 5 FPS


            var world = Helpers.NewWorld();

            var inputSystem = new PlayerInputSystem();
            var movementSystem = new MovementSystem();

            var game = new GameManager(
                fixedTick: fixedDeltaTime,
                world: world,
                update: 
                new Systems(world)
                    .Add(inputSystem),
                fixedUpdate:
                new Systems(world)
                    .Add(movementSystem));

            game.Create();

            var query = world.GetEntityQuery<EntityQuery<SampleStructs.Foo>>();

            var entityInput = world.NewEntity();
            //ref var input = ref entityInput.GetComponent<SingletonInputComponent>();

            var entityPlayer = world.NewEntity();
            entityPlayer.GetComponent<SingletonPlayerComponent>();
            ref var movement = ref entityPlayer.GetComponent<MovementComponent>();
            ref var position = ref entityPlayer.GetComponent<PositionComponent>();



            int inputIdx = 0;
            var inputs = new InputAtTime[]
            {
                new InputAtTime
                {
                    Time = 0.2f,
                    Input = new SingletonInputComponent
                    {
                        isRightDown = true
                    }
                },
                new InputAtTime
                {
                    Time = 0.7f,
                    Input = new SingletonInputComponent
                    {
                        isRightUp = true
                    }
                },
                new InputAtTime
                {
                    Time = 1.0f,
                    Input = new SingletonInputComponent
                    {
                        isLeftDown = true
                    }
                },
                new InputAtTime
                {
                    Time = 1.5f,
                    Input = new SingletonInputComponent
                    {
                        isLeftUp = true
                    }
                },
            };



            int frameNumber = 0;

            for (float time = 0; time <= 2.0f; time += deltaTime)
            {
                // New input.
                var input = new SingletonInputComponent();

                for (; inputIdx < inputs.Length; inputIdx++)
                {
                    if (inputs[inputIdx].Time > time)
                    {
                        break;
                    }

                    input = inputs[inputIdx].Input;
                }

                entityInput.GetComponent<SingletonInputComponent>() = input;

                Debug.WriteLine($"{frameNumber++}:{time:N1}: LDown={input.isLeftDown}, LUp={input.isLeftUp}, RDown={input.isRightDown}, RUp={input.isRightUp}");

                game.Run(deltaTime);

                Debug.WriteLine($"Position={position.x}");
            }



            WorldState stateBackup = new WorldState();

            world.State.CopyStateTo(ref stateBackup);

            var positionBefore = position; 


            game.Rewind(5);
            game.PlayForward(5, ref position);

            var positionAfter1 = position;

            Assert.True(positionBefore.x.AboutEquals(positionAfter1.x));

            game.Rewind(3);
            game.PlayForward(3, ref position);

            var positionAfter2 = position;

            Assert.True(positionBefore.x.AboutEquals(positionAfter2.x));

            game.Rewind(5);

            // Update position.
            position.x += 5.0f;

            game.PlayForward(5, ref position);

            var positionAfter3 = position;

            Assert.True((positionBefore.x + 5.0f).AboutEquals(positionAfter3.x));
        }

        private class GameManager
        {
            public const int SnapShotCount = 10; // in fixed update frames.

            internal readonly float FixedTick;

            internal readonly World World;

            internal readonly Systems Update;
            internal readonly Systems FixedUpdate;

            private float _lastFixedTime = 0;
            private float _time = 0;

            private readonly SnapShots _snapShots;

            private const float TickEpsilon = 0.000001f;

            public GameManager(
                float fixedTick,
                World world,
                Systems update,
                Systems fixedUpdate)
            {
                FixedTick = fixedTick;
                World = world;
                Update = update;
                FixedUpdate = fixedUpdate;

                _snapShots = new SnapShots(SnapShotCount);
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

            public void PlayForward(int fixedFrameCount, ref PositionComponent position)
            {
                // Clone the snapshot inputs
                var clonedInputs = new AppendOnlyList<AppendOnlyList<SnapShot.InputFrame>>(fixedFrameCount);

                for (int i = 0; i < fixedFrameCount; i++)
                {
                    SnapShot replaySnapShot = _snapShots.Peek(i);

                    clonedInputs.Add(new AppendOnlyList<SnapShot.InputFrame>(replaySnapShot.Inputs.Count));

                    replaySnapShot.MoveInputsTo(ref clonedInputs.Items[i]);
                }

                for (int i = 0; i < fixedFrameCount; i++)
                {
                    // Replay inputs and simulate.

                    float tickRemaining = FixedTick;

                    var query = (EntityQuery<SingletonInputComponent>)World.GetEntityQuery<EntityQuery<SingletonInputComponent>>();
                    ref var input = ref query.GetSingleton();

                    // Apply all inputs for this fixed update
                    for (int j = 0; j < clonedInputs.Items[i].Count; j++)
                    {
                        input = clonedInputs.Items[i].Items[j].Input;

                        float deltaTime = clonedInputs.Items[i].Items[j].Time - _time;

Debug.WriteLine($"{i}:{_time:N1}: LDown={input.isLeftDown}, LUp={input.isLeftUp}, RDown={input.isRightDown}, RUp={input.isRightUp}");

                        Run(deltaTime);

                        tickRemaining -= deltaTime;

Debug.WriteLine($"Position={position.x}");
                    }

                    if (tickRemaining >- TickEpsilon)
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
                var query = (EntityQuery<SingletonInputComponent>)World.GetEntityQuery<EntityQuery<SingletonInputComponent>>();
                ref readonly var input = ref query.GetSingletonComponentReadonly();

                _snapShots.Current()
                    .Inputs
                    .Add(new SnapShot.InputFrame
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

            /// <summary>
            /// Ring buffer of World snapshots.
            /// </summary>
            private class SnapShots
            {
                private readonly int Count;

                private int _current;
                private SnapShot[] _snapShots;

                public SnapShots(int count)
                {
                    Count = count;

                    _current = 0;
                    _snapShots = new SnapShot[Count];

                    for (int i = 0; i < _snapShots.Length; i++)
                    {
                        _snapShots[i] = new SnapShot
                        {
                            Inputs = new AppendOnlyList<SnapShot.InputFrame>(capacity: 8)
                        };
                    }
                }

                /// <summary>
                /// Ends the current snapshot and begins the next.
                /// </summary>
                public void NextSnapshot(float fixedTime, World world)
                {
                    _snapShots[_current].Time = fixedTime;

                    // Advance to next snapshot. Reset it.
                    MoveNext();

                    world.State.CopyStateTo(ref _snapShots[_current].WorldState);
                }

                /// <summary>
                /// Returns the current snapshot.
                /// </summary>
                public ref readonly SnapShot Current()
                {
                    return ref _snapShots[_current];
                }

                public void Seek(int frameCount)
                {
                    if (Math.Abs(frameCount) >= Count)
                    {
                        throw new InvalidOperationException($"Seek frame count too large. {nameof(frameCount)}={frameCount}, Count={Count}");
                    }

                    _current = (_current + SnapShotCount + frameCount) % SnapShotCount;
                }

                public ref readonly SnapShot Peek(int frameCount)
                {
                    if (frameCount < 0 ||
                        Math.Abs(frameCount) >= Count)
                    {
                        throw new InvalidOperationException($"Invalid Peek frame count. {nameof(frameCount)}={frameCount}, Count={Count}");
                    }

                    var peekPos = (_current + SnapShotCount + frameCount) % SnapShotCount;

                    return ref _snapShots[peekPos];
                }

                /// <summary>
                /// Move to the next snapshot position. The snapshot will be reset.
                /// </summary>
                private void MoveNext()
                {
                    Seek(1);
                    Current().Reset();
                }
            }

            private struct SnapShot
            {
                public AppendOnlyList<InputFrame> Inputs;
                public WorldState WorldState;
                public float Time;

                public void Reset()
                {
                    Inputs.Resize(0);
                }

                public void MoveInputsTo(ref AppendOnlyList<InputFrame> inputs)
                {
                    this.Inputs.ShallowCopyTo(inputs);

                    this.Inputs.Count = 0;
                }

                public struct InputFrame
                {
                    public float Time;
                    public SingletonInputComponent Input;
                }
            }
        }

        //
        // Misc
        //

        internal struct InputAtTime
        {
            public float Time;
            public SingletonInputComponent Input;
        }

        //
        // Components
        //

        internal struct MovementComponent
        {
            public float velocity_x;
        }

        internal struct PositionComponent
        {
            public float x;
        }

        internal struct SingletonPlayerComponent
        {
            // Tag
        }

        internal struct SingletonInputComponent
        {
            public bool
                isRightDown,
                isRightUp,
                isLeftDown,
                isLeftUp;
        }

        //
        // Systems
        //

        private class PlayerInputSystem : SystemBase
        {
            public EntityQuery<SingletonInputComponent> SingletoneInputQuery = null;
            public EntityQuery<SingletonPlayerComponent> SingletonPlayerQuery = null;

            public override void OnUpdate(float deltaTime)
            {
                ref readonly var input = ref SingletoneInputQuery.GetSingletonComponentReadonly();

                Entity playerEnt = default;

                foreach (var entity in SingletonPlayerQuery)
                {
                    playerEnt = entity;

                    break;
                }

                if (input.isLeftDown || input.isRightDown)
                {
                    ref var movement = ref playerEnt.GetComponent<MovementComponent>();

                    movement.velocity_x =
                        5.0f
                        * deltaTime
                        * (input.isLeftDown ? -1.0f : 1.0f);
                }
                else if(input.isLeftUp || input.isRightUp)
                {
                    ref var movement = ref playerEnt.GetComponent<MovementComponent>();

                    movement.velocity_x = 0;
                }
            }
        }

        private class MovementSystem : SystemBase
        {
            public EntityQuery<MovementComponent> MovementQuery = null;

            public override void OnUpdate(float deltaTime)
            {
                foreach (var entity in MovementQuery)
                {
                    ref var movement = ref entity.GetComponent<MovementComponent>();

                    if (Math.Abs(movement.velocity_x) < 0.001f)
                    {
                        movement.velocity_x = 0;

                        continue;
                    }

                    ref var position = ref entity.GetComponent<PositionComponent>();

                    // Move position by velocity.
                    position.x += movement.velocity_x;

                    // Decelerate velocity;
                    //movement.velocity_x -=
                    //    Math.Sign(movement.velocity_x)
                    //    * 2.5f
                    //    * deltaTime;
                }
            }
        }
    }
}
