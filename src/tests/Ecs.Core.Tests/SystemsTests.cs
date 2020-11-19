using System.Collections.Generic;
using Xunit;

namespace Ecs.Core.Tests
{
    public class SystemsTests
    {
        /// <summary>
        /// Validates custom dependency injection.
        /// </summary>
        [Fact]
        public void DependencyInjection()
        {
            HashSet<int> set = new HashSet<int>();

            var world = Helpers.NewWorld();
            var systems =
                new Systems(world)
                    .Add(new SystemDependencyInjection())
                    .Inject(new SystemDependencyInjection.FooClass(set));

            systems.Create();

            Assert.DoesNotContain(2, set);

            systems.Run(1);

            Assert.Contains(2, set);
        }


        [Fact]
        public void TwoSystemsOneWorld()
        {
            var systemFoo = new SystemFoo();

            var world = Helpers.NewWorld();
            var systems1 =
                new Systems(world)
                    .Add(systemFoo);
            systems1.Create();

            var query = world.GetEntityQuery<EntityQuery<SampleStructs.Foo>>();

            var systems2 =
                new Systems(world)
                    .Add(systemFoo);
            systems2.Create();

            var entity = world.NewEntity();
            ref var comp = ref entity.GetComponent<SampleStructs.Foo>();
            comp.x = 1;

            systems1.Run(1);

            Assert.Equal(2, comp.x);
            Assert.Equal(1, query.GetEntityCount());

            systems2.Run(1);

            Assert.Equal(3, comp.x);
            Assert.Equal(1, query.GetEntityCount());
        }

        [Fact]
        public void ActiveInactive()
        {
            var systemFoo = new SystemFoo();

            var systems =
                new Systems(Helpers.NewWorld())
                    .Add(systemFoo);
            systems.Create();

            var entity = systems.World.NewEntity();

            ref var foo = ref entity.GetComponent<SampleStructs.Foo>();
            foo.x = 1;

            systems.Run(1);

            Assert.Equal(2, foo.x);

            systems.SetActive(systemFoo, isActive: false);

            systems.Run(1);

            Assert.Equal(2, foo.x);

            systems.SetActive(systemFoo, isActive: true);

            systems.Run(1);

            Assert.Equal(3, foo.x);

            // For good measure verify OnCreate called once.
            Assert.Equal(1, systemFoo.OnCreateCallCounter);
        }

        [Fact]
        public void SystemsAndEntitiesAndQueries()
        {
            var systemFoo = new SystemFoo();

            var systems =
                new Systems(Helpers.NewWorld())
                    .Add(systemFoo);
            systems.Create();

            var entity1 = systems.World.NewEntity();
            var entity2 = systems.World.NewEntity();

            ref var e1CompFoo = ref entity1.GetComponent<SampleStructs.Foo>();
            e1CompFoo.x = 1;
            ref var e1CompBar = ref entity1.GetComponent<SampleStructs.Bar>();
            e1CompBar.a = 11;

            ref var e2CompFoo = ref entity2.GetComponent<SampleStructs.Foo>();
            e2CompFoo.x = 2;
            ref var e2CompBar = ref entity2.GetComponent<SampleStructs.Bar>();
            e2CompBar.a = 12;

            systems.Run(1);

            Assert.False(entity1.IsFreed());
            Assert.Equal(2, systemFoo.QueryFoo.GetEntityCount());
            Assert.Equal(2, e1CompFoo.x);
            Assert.Equal(3, e2CompFoo.x);
            Assert.Equal(12, e1CompBar.a);
            Assert.Equal(13, e2CompBar.a);

            entity1.RemoveComponent<SampleStructs.Foo>();

            systems.Run(1);

            Assert.False(entity1.IsFreed());
            Assert.Equal(1, systemFoo.QueryFoo.GetEntityCount());
            Assert.Equal(4, e2CompFoo.x);
            Assert.Equal(13, e1CompBar.a);
            Assert.Equal(14, e2CompBar.a);

            entity1.RemoveComponent<SampleStructs.Bar>();

            Assert.True(entity1.IsFreed());
        }

        [Fact]
        public void System_OnCreate()
        {
            int onCreateCounter = 0;

            var systems = new Systems(Helpers.NewWorld());
            systems.Add(
                new SystemWithCallbacksAndQuery<SampleStructs.Foo>
                {
                    OnCreateAction = (system) =>
                    {
                        onCreateCounter++;
                    }
                });

            systems.Create();

            systems.Run(1);
            systems.Run(1);

            Assert.Equal(1, onCreateCounter);
        }

        /// <summary>
        /// Validate single frame component.
        /// </summary>
        [Fact]
        public void SingleFrame()
        {
            bool addSingleFrameComponent = true;
            bool hadSingleFrameComponent = false;

            var systems = new Systems(Helpers.NewWorld());
            systems
                .Add(
                    new SystemWithCallbacksAndQuery<Entity, SampleStructs.Foo>
                    {
                        OnCreateAction = (system) =>
                        {
                            // Create entity with 1 component
                            var entity = systems.World.NewEntity();
                            entity.GetComponent<SampleStructs.Foo>();

                            system.Data["entity"] = entity;
                        },
                        OnUpdateAction = (system, dt) =>
                        {
                            if (addSingleFrameComponent)
                            {
                                system.Data["entity"].GetComponent<SampleStructs.Bar>();
                            }
                        }
                    })
                .Add(
                    new SystemWithCallbacksAndQuery<Entity, SampleStructs.Bar>
                    {
                        OnUpdateAction = (system, dt) =>
                        {
                            hadSingleFrameComponent = false;

                            foreach (var entity in system.Query)
                            {
                                hadSingleFrameComponent = true;
                            }
                        }
                    })
                .SingleFrame<SampleStructs.Bar>()
                .Create();

            systems.Run(1);

            // Should have single frame component during first run.
            Assert.True(hadSingleFrameComponent);
            // Stop adding single frame component.
            addSingleFrameComponent = false;

            systems.Run(1);
            // Single frame component now removed.
            Assert.False(hadSingleFrameComponent);
        }

        private class SystemFoo : SystemBase
        {
            public EntityQuery<SampleStructs.Foo> QueryFoo = null;
            public EntityQuery<SampleStructs.Bar> QueryBar = null;

            public int OnCreateCallCounter = 0;

            public override void OnCreate()
            {
                OnCreateCallCounter++;
            }

            public override void OnUpdate(float deltaTime)
            {
                foreach (int index in QueryFoo)
                {
                    ref SampleStructs.Foo foo = ref QueryFoo.Get(index);

                    foo.x++;
                }

                foreach (int index in QueryBar)
                {
                    ref SampleStructs.Bar bar = ref QueryBar.Get(index);

                    bar.a++;
                }
            }
        }

        private class SystemDependencyInjection : SystemBase
        {
            public EntityQuery<SampleStructs.Foo> QueryFoo = null;
            public FooClass Foo = null;

            public override void OnUpdate(float deltaTime)
            {
                Foo.Set(2);
            }

            public class FooClass
            {
                readonly HashSet<int> _set;

                public FooClass(HashSet<int> set)
                {
                    _set = set;
                }

                public void Set(int index)
                {
                    _set.Add(index);
                }
            }
        }
    }
}