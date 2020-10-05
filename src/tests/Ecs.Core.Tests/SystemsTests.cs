using System;
using Xunit;

namespace Ecs.Core.Tests
{
    public class SystemsTests
    {
        [Fact]
        public void EndToEnd()
        {
            var systemFoo = new SystemFoo();

            var systems = new Systems(new World(EcsConfig.Default));
            systems.Add(systemFoo);

            systems.Init();

            var entity1 = systems.World.NewEntity();
            var entity2 = systems.World.NewEntity();

            ref var e1CompFoo = ref entity1.GetComponent<SampleStructs.FooData>();
            e1CompFoo.x = 1;
            ref var e1CompBar = ref entity1.GetComponent<SampleStructs.BarData>();

            ref var e2CompFoo = ref entity2.GetComponent<SampleStructs.FooData>();
            e2CompFoo.x = 2;

            ref var e2CompBar = ref entity2.GetComponent<SampleStructs.BarData>();

            systems.Run();

            Assert.False(entity1.IsFreed());
            Assert.Equal(2, systemFoo.QueryA.GetEntityCount());
            Assert.Equal(2, e1CompFoo.x);
            Assert.Equal(3, e2CompFoo.x);

            entity1.RemoveComponent<SampleStructs.FooData>();

            systems.Run();

            Assert.False(entity1.IsFreed());
            Assert.Equal(1, systemFoo.QueryA.GetEntityCount());
            Assert.Equal(4, e2CompFoo.x);

            entity1.RemoveComponent<SampleStructs.BarData>();

            Assert.True(entity1.IsFreed());
        }

        private class SystemFoo : SystemBase
        {
            public EntityQuery QueryA = new EntityQuery(
                new Type[] { typeof(SampleStructs.FooData) });

            public override void OnUpdate()
            {
                for (int i  = 0; i < QueryA.GetEntityCount(); i++)
                {
                    var entity = QueryA.GetEntity(i);

                    ref var foo = ref entity.GetComponent<SampleStructs.FooData>();

                    foo.x++;
                }
            }
        }
    }
}
