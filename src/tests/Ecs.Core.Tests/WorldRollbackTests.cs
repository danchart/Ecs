using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

            var world = Helpers.NewWorld();

            var inputSystem = new PlayerInputSystem();
            var movementSystem = new MovementSystem();

            var game = new GameManager(
                fixedTick: fixedDeltaTime,
                world: world,
                update: 
                new Systems(world)
                    .Add(inputSystem)
                    .Add(new ClearInputSystem()),
                fixedUpdate:
                new Systems(world)
                    .Add(movementSystem));

            game.Create();

            var query = world.GetEntityQuery<EntityQuery<SampleStructs.Foo>>();

            var entityInput = world.NewEntity();
            ref var input = ref entityInput.GetComponent<SingletonInputComponent>();

            var entityPlayer = world.NewEntity();
            entityPlayer.GetComponent<SingletonPlayerComponent>();
            ref var movement = ref entityPlayer.GetComponent<MovementComponent>();
            ref var position = ref entityPlayer.GetComponent<PositionComponent>();



            var inputIdx = 0;
            var inputs = new InputAtTime[]
            {
                new InputAtTime
                {
                    StartTime = 0.2f,
                    EndTime = 0.7f,
                    Input = new SingletonInputComponent
                    {
                        isMoveRightDown = true
                    }
                },
                new InputAtTime
                {
                    StartTime = 1.0f,
                    EndTime = 1.5f,
                    Input = new SingletonInputComponent
                    {
                        isMoveLeftDown = true
                    }
                },
            };





            for (float time = 0; time < 2.0f; time += deltaTime)
            {
                input.isMoveLeftDown = default;
                input.isMoveRightDown = default;

                const float timeSkew = 0.01f;

                for (int i = inputIdx; i < inputs.Length; i++)
                {
                    if (inputs[i].StartTime - timeSkew > time)
                    {
                        continue;
                    }

                    if (inputs[i].EndTime + timeSkew < time)
                    {
                        inputIdx++;
                        continue;
                    }

                    input.isMoveLeftDown |= inputs[i].Input.isMoveLeftDown;
                    input.isMoveRightDown |= inputs[i].Input.isMoveRightDown;
                }

                Debug.WriteLine($"{time:N1}: Left={input.isMoveLeftDown}, Right={input.isMoveRightDown}");

                game.Run(deltaTime);

                Debug.WriteLine($"Position={position.x}");
            }



            WorldState stateBackup = new WorldState();

            world.State.CopyState(ref stateBackup);


            game.Rewind(10);


            int ii = 0;
        }

        private class GameManager
        {
            public const int SnapShotCount = 10; // in fixed update frames.

            internal readonly float FixedTick;

            internal World World;

            internal Systems Update;
            internal Systems FixedUpdate;

            private float _lastFixedTime = 0;
            private float _time = 0;

            private int _currentSnapShot = 0;
            private SnapShot[] SnapShots = new SnapShot[SnapShotCount];


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
            }

            public void Create()
            {
                Update.Create();
                FixedUpdate.Create();

                for(int i = 0; i < SnapShots.Length; i++)
                {
                    SnapShots[i] = new SnapShot();
                    SnapShots[i].Reset();
                }
            }

            public void Rewind(int fixedFrameCount)
            {
                // Rewind world state
                _currentSnapShot = (_currentSnapShot + SnapShotCount - fixedFrameCount) % SnapShotCount;
                SnapShots[_currentSnapShot].WorldState.CopyState(ref World.State);

                // Rewind clock
                _time -= (fixedFrameCount * FixedTick);

                for (int i = 0; i < fixedFrameCount; i++)
                {
                    // Replay inputs and simulate.
                    float tickRemaining = FixedTick;

                    // Apply all inputs from between fixed frame updates.
                    var query = (EntityQuery<SingletonInputComponent>)World.GetEntityQuery<EntityQuery<SingletonInputComponent>>();
                    foreach (var entity in query)
                    {
                        ref var input = ref entity.GetComponent<SingletonInputComponent>();

                        for (int j = 0; j < SnapShots[_currentSnapShot].Inputs.Count; j++)
                        {
                            input = SnapShots[_currentSnapShot].Inputs.Items[j].Input;

                            float deltaTime = SnapShots[_currentSnapShot].Inputs.Items[j].Time - _time;

                            Run(deltaTime);

                            // Advance time
                            _time = SnapShots[_currentSnapShot].Inputs.Items[j].Time;

                            tickRemaining -= deltaTime;
                        }

                        // singleton
                        break;
                    }

                    Run(tickRemaining);
                }
            }

            /// <summary>
            ///  Simulated update loop.
            /// </summary>
            public void Run(float deltaTime)
            {
                _time += deltaTime;

                // Capture input to the current snapshot
                var query = (EntityQuery<SingletonInputComponent>)World.GetEntityQuery<EntityQuery<SingletonInputComponent>>();
                foreach (var entity in query)
                {
                    ref readonly var input = ref entity.GetReadOnlyComponent<SingletonInputComponent>();

                    SnapShots[_currentSnapShot]
                        .Inputs
                        .Add(new SnapShot.InputFrame
                        {
                            Time = _time,
                            Input = input,
                        });

                    // Singleton
                    break;
                }

                while (_time - _lastFixedTime >= FixedTick)
                {
                    FixedUpdate.Run(FixedTick);

                    _lastFixedTime += FixedTick;

                    // Copy World State to the current snapshot.
                    World.State.CopyState(ref SnapShots[_currentSnapShot].WorldState);

                    // Move to next snapshot. Reset it.
                    _currentSnapShot = (_currentSnapShot + 1) % SnapShotCount;
                    SnapShots[_currentSnapShot].Reset();
                }

                Update.Run(deltaTime);
            }

            private class SnapShot
            {
                public AppendOnlyList<InputFrame> Inputs;
                public WorldState WorldState = new WorldState();

                public void Reset()
                {
                    Inputs = new AppendOnlyList<InputFrame>(capacity: 8);
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
            public float StartTime;
            public float EndTime;
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
                isMoveRightDown,
                isMoveLeftDown;
        }

        //
        // Systems
        //

        /// <summary>
        /// Reset inputs every update.
        /// </summary>
        private class ClearInputSystem : SystemBase
        {
            public EntityQuery<SingletonInputComponent> SingletoneInputQuery = null;

            public override void OnUpdate(float deltaTime)
            {
                foreach (var entity in SingletoneInputQuery)
                {
                    ref var input = ref entity.GetComponent<SingletonInputComponent>();

                    input = default;
                }
            }
        }

        private class PlayerInputSystem : SystemBase
        {
            public EntityQuery<SingletonInputComponent> SingletoneInputQuery = null;
            public EntityQuery<SingletonPlayerComponent> SingletonPlayerQuery = null;

            public override void OnUpdate(float deltaTime)
            {
                ref readonly var input = ref SingletoneInputQuery.Get();

                Entity playerEnt = default;

                foreach (var entity in SingletonPlayerQuery)
                {
                    playerEnt = entity;

                    break;
                }

                if (input.isMoveLeftDown || input.isMoveRightDown)
                {
                    ref var movement = ref playerEnt.GetComponent<MovementComponent>();

                    movement.velocity_x =
                        5.0f
                        * deltaTime
                        * (input.isMoveLeftDown ? -1.0f : 1.0f);

                    //movement.velocity_x =
                    //    Math.Min(
                    //        5.0f,
                    //        movement.velocity_x
                    //        +
                    //            (5.0f
                    //            * deltaTime
                    //            * (input.isMoveLeftDown ? -1.0f : 1.0f)));
                }
            }
        }

        private class MovementSystem : SystemBase
        {
            public EntityQueryWithChangeFilter<MovementComponent> MovementQuery = null;

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
                    movement.velocity_x -=
                        Math.Sign(movement.velocity_x)
                        * 2.5f
                        * deltaTime;
                }
            }
        }
    }
}
