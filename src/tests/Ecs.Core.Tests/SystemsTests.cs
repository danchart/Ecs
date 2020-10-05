using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Ecs.Core.Tests
{
    public class SystemsTests
    {
        [Fact]
        public void SystemsAndEntitiesAndQueries()
        {
            var systemFoo = new SystemFoo();

            var systems = new Systems(Helpers.NewWorld());
            systems.Add(systemFoo);

            systems.Init();

            var entity1 = systems.World.NewEntity();
            var entity2 = systems.World.NewEntity();

            ref var e1CompFoo = ref entity1.GetComponent<SampleStructs.FooData>();
            e1CompFoo.x = 1;
            ref var e1CompBar = ref entity1.GetComponent<SampleStructs.BarData>();
            e1CompBar.a = 11;

            ref var e2CompFoo = ref entity2.GetComponent<SampleStructs.FooData>();
            e2CompFoo.x = 2;
            ref var e2CompBar = ref entity2.GetComponent<SampleStructs.BarData>();
            e2CompBar.a = 12;

            systems.Run(1);

            Assert.False(entity1.IsFreed());
            Assert.Equal(2, systemFoo.QueryFoo.GetEntityCount());
            Assert.Equal(2, e1CompFoo.x);
            Assert.Equal(3, e2CompFoo.x);
            Assert.Equal(12, e1CompBar.a);
            Assert.Equal(13, e2CompBar.a);

            entity1.RemoveComponent<SampleStructs.FooData>();

            systems.Run(1);

            Assert.False(entity1.IsFreed());
            Assert.Equal(1, systemFoo.QueryFoo.GetEntityCount());
            Assert.Equal(4, e2CompFoo.x);
            Assert.Equal(13, e1CompBar.a);
            Assert.Equal(14, e2CompBar.a);

            entity1.RemoveComponent<SampleStructs.BarData>();

            Assert.True(entity1.IsFreed());
        }

        [Fact]
        public void System_OnCreate()
        {
            int onCreateCounter = 0;

            var systems = new Systems(Helpers.NewWorld());
            systems.Add(
                new SystemOnCreate
                {
                    OnCreateAction = () =>
                    {
                        onCreateCounter++;
                    }
                });

            systems.Init();

            systems.Run(1);
            systems.Run(1);

            Assert.Equal(1, onCreateCounter);
        }

        private class SystemFoo : SystemBase
        {
            public EntityQuery QueryFoo = 
                new EntityQuery(
                    new Type[] { typeof(SampleStructs.FooData) });
            public EntityQuery QueryBar = new EntityQuery<SampleStructs.BarData>();

            public override void OnUpdate(float deltaTime)
            {
                for (int i  = 0; i < QueryFoo.GetEntityCount(); i++)
                {
                    var entity = QueryFoo.GetEntity(i);

                    ref var foo = ref entity.GetComponent<SampleStructs.FooData>();

                    foo.x++;
                }

                foreach (var index in QueryBar)
                {
                    var entity = QueryBar.GetEntity(index);
                    ref var bar = ref entity.GetComponent<SampleStructs.BarData>();

                    bar.a++;
                }
            }
        }

        private class SystemOnCreate : SystemBase
        {
            public Action OnCreateAction;

            public override void OnCreate()
            {
                OnCreateAction?.Invoke();
            }
        }
    }
}
