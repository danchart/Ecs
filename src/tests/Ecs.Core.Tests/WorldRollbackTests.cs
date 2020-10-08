using System.Runtime.Serialization.Formatters;
using Xunit;

namespace Ecs.Core.Tests
{
    public class WorldRollbackTests
    {
        [Fact]
        public void Rollback()
        {
            var systemFoo = new SystemFoo();

            var world = Helpers.NewWorld();
            var systems1 =
                new Systems(world)
                    .Add(systemFoo);
            systems1.Create();

            var query = world.GetEntityQuery<EntityQuery<SampleStructs.Foo>>();

            var entityInput = world.NewEntity();
            ref var input = ref entityInput.GetComponent<SingletonInputComponent>();

            var entityPlayer = world.NewEntity();            
            entityPlayer.GetComponent<PlayerComponent>();
            ref var movement = ref entityPlayer.GetComponent<MovementComponent>();
            ref var position = ref entityPlayer.GetComponent<PositionComponent>();

            systems1.Run(1);

            Assert.Equal(2, comp.x);
            Assert.Equal(1, query.GetEntityCount());

            systems2.Run(1);

            Assert.Equal(3, comp.x);
            Assert.Equal(1, query.GetEntityCount());
        }

        internal struct MovementComponent
        {
            int velocity_x;
        }

        internal struct PositionComponent
        {
            int x;
        }

        internal struct SingletonPlayerComponent
        {
            // Tag
        }

        internal struct SingletonInputComponent
        {
            bool
                isMoveRightDown,
                isMoveLeftDown;
        }

        private class PlayerInputSystem : SystemBase
        {
            public EntityQuery<SingletonInputComponent> SingletoneInputQuery = null;
            public EntityQuery<SingletonPlayerComponent> SingletonPlayerQuery = null;

            public override void OnCreate()
            {
            }

            public override void OnUpdate(float deltaTime)
            {
                foreach (var entity in QueryFoo)
                {
                    ref var foo = ref entity.GetComponent<SampleStructs.Foo>();

                    foo.x++;
                }

                foreach (var entity in QueryBar)
                {
                    ref var bar = ref entity.GetComponent<SampleStructs.Bar>();

                    bar.a++;
                }
            }
        }
    }
}
