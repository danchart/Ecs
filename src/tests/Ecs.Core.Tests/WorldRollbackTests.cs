using System;
using Xunit;

namespace Ecs.Core.Tests
{
    public class WorldRollbackTests
    {
        internal const float Tick = 1f / 60f; // 60Hz
        internal const float FixedTick = 1f / 60f; // 60Hz

        private Systems _update;
        private Systems _fixedUpdate;

        private float _lastFixedTime;
        private float _lastTime;
        private float _time;

        [Fact]
        public void Rollback()
        {
            _time = 0;

            var inputSystem = new PlayerInputSystem();

            var world = Helpers.NewWorld();
            _update =
                new Systems(world)
                    .Add(inputSystem)
                    .Add(new ClearInputSystem());

            _update.Create();

            var movementSystem = new MovementSystem();

            _fixedUpdate =
                new Systems(world)
                    .Add(movementSystem);

            _fixedUpdate.Create();

            var query = world.GetEntityQuery<EntityQuery<SampleStructs.Foo>>();

            var entityInput = world.NewEntity();
            ref var input = ref entityInput.GetComponent<SingletonInputComponent>();

            var entityPlayer = world.NewEntity();
            entityPlayer.GetComponent<SingletonPlayerComponent>();
            ref var movement = ref entityPlayer.GetComponent<MovementComponent>();
            ref var position = ref entityPlayer.GetComponent<PositionComponent>();

            float deltaTime = 1f / 90f; // Running 90 FPS

            input.isMoveRightDown = true;

            _time += deltaTime;
            Run();
            
            Assert.Equal(0, position.x);

            _time += deltaTime;
            Run();

            Assert.NotEqual(0, position.x);
        }

        private void Run()
        {
            var deltaTime = _time - _lastTime;

            while (_time - _lastFixedTime > FixedTick)
            {
                _fixedUpdate.Run(FixedTick);

                _lastFixedTime += FixedTick;
            }

            _update.Run(deltaTime);

            _lastTime = _time;
        }

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
                        Math.Max(
                            5.0f,
                            movement.velocity_x
                            +
                                (5.0f
                                * deltaTime
                                * (input.isMoveLeftDown ? -1.0f : 1.0f)));
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

                    if (movement.velocity_x < 0.001f)
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
