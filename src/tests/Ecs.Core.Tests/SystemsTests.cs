﻿using System;
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
                new SystemWithCallbacks<SampleStructs.Foo>
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

        [Fact]
        public void SingleFrame()
        {
            bool addSingleFrameComponent = true;
            bool hadSingleFrameComponent = false;

            var systems = new Systems(Helpers.NewWorld());
            systems
                .Add(
                    new SystemWithCallbacks<Entity, SampleStructs.Foo> 
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
                    new SystemWithCallbacks<Entity, SampleStructs.Bar>
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

            Assert.True(hadSingleFrameComponent);

            systems.Run(1);

            Assert.False(hadSingleFrameComponent);

        }

        private class SystemFoo : SystemBase
        {
            public EntityQuery<SampleStructs.Foo> QueryFoo = null;
            public EntityQuery<SampleStructs.Bar> QueryBar = null;

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
                    //ref readonly var bar = ref entity.GetComponent<SampleStructs.BarData>();

                    bar.a++;
                }
            }
        }
    }
}
