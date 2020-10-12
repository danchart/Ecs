﻿using Ecs.Core;
using System;
using System.Diagnostics;
using Xunit;

namespace Ecs.Simulation.Tests
{
    public class SimulationRollbackTests
    {
        [Fact]
        public void Rollback()
        {
            const float deltaTime = 1f / 10f; // Running 10 FPS
            const float fixedDeltaTime = 1f / 5f; // Fixed tick at 5 FPS

            var world = new World(EcsConfig.Default);

            var inputSystem = new PlayerInputSystem();
            var movementSystem = new MovementSystem();

            var game = new SimulationManager<SingletonInputComponent>(
                fixedTick: fixedDeltaTime,
                world: world,
                update:
                new Systems(world)
                    .Add(inputSystem),
                fixedUpdate:
                new Systems(world)
                    .Add(movementSystem));

            game.Create();

            // Singleton input entity
            var entityInput = world.NewEntity();
            // Singleton player entity.
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
            game.PlayForward(5);

            var positionAfter1 = position;

            Assert.True(positionBefore.x.AboutEquals(positionAfter1.x));

            game.Rewind(3);
            game.PlayForward(3);

            var positionAfter2 = position;

            Assert.True(positionBefore.x.AboutEquals(positionAfter2.x));

            game.Rewind(5);

            // Update position.
            position.x += 5.0f;

            game.PlayForward(5);

            var positionAfter3 = position;

            Assert.True((positionBefore.x + 5.0f).AboutEquals(positionAfter3.x));
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
                else if (input.isLeftUp || input.isRightUp)
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
