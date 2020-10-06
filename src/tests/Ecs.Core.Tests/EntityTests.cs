using System.Collections.Generic;
using Xunit;

namespace Ecs.Core.Tests
{
    public class EntityTests
    {
        [Fact]
        public void Equality()
        {
            var world = Helpers.NewWorld();

            var entity1 = world.NewEntity();
            var entity2 = world.NewEntity();

            var set = new HashSet<Entity>
            {
                entity1, entity2
            };

            Assert.NotEqual(entity1, entity2);
            Assert.True(entity1 != entity2);
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.True(entity1 == entity1);
#pragma warning restore CS1718 // Comparison made to same variable
            Assert.True(entity1.Equals(entity1));
            Assert.False(entity1.Equals(entity2));
            Assert.Equal(2, set.Count);
        }

        [Fact]
        public void Free()
        {
            var world = Helpers.NewWorld();

            var entity1 = world.NewEntity();
            entity1.GetComponent<SampleStructs.FooData>();
            var entity2 = world.NewEntity();
            entity2.GetComponent<SampleStructs.BarData>();

            Assert.False(entity1.IsFreed());
            Assert.False(entity2.IsFreed());

            entity1.Free();

            Assert.True(entity1.IsFreed());
            Assert.False(entity2.IsFreed());
        }

        /// <summary>
        /// Validate entity component array resize
        /// </summary>
        [Fact]
        public void Resize()
        {
            var config = EcsConfig.Default;
            // Initial array count of 1
            config.InitialEntityComponentCapacity = 1;

            var world = new World(config);

            var entity = world.NewEntity();

            // Add 2 components

            ref var foo = ref entity.GetComponent<SampleStructs.FooData>();
            // Resize should happen here.
            ref var bar = ref entity.GetComponent<SampleStructs.BarData>();
        }

        [Fact]
        public void ComponentRef()
        {
            var world = Helpers.NewWorld();

            var entity1 = world.NewEntity();
            ref var compFoo = ref entity1.GetComponent<SampleStructs.FooData>();
            compFoo.x = 1;

            var compRefFoo = entity1.Reference<SampleStructs.FooData>();
            ref var compFooFromRef = ref compRefFoo.Unref();

            compFooFromRef.x = 2;

            Assert.Equal(compFoo.x, compFooFromRef.x);
        }

        [Fact]
        public void GetVersion()
        {
            var systems = new Systems(Helpers.NewWorld());
            var system = new GetVersionSystem<SampleStructs.FooData>();
            systems
                .Add(system)
                .Init();

            var entity = systems.World.NewEntity();
            entity.GetComponent<SampleStructs.FooData>();

            var version1 = entity.GetComponentVersion<SampleStructs.FooData>();

            systems.Run(1);

            var version2 = entity.GetComponentVersion<SampleStructs.FooData>();

            Assert.NotEqual(version1, version2);
            Assert.True(system.WasComponentModified);

            system.UseReadOnly = true;
            systems.Run(1);
            var version3 = entity.GetComponentVersion<SampleStructs.FooData>();

            Assert.Equal(version2, version3);
            Assert.False(system.WasComponentModified);

            system.UseReadOnly = false;
            systems.Run(1);
            var version4 = entity.GetComponentVersion<SampleStructs.FooData>();

            Assert.NotEqual(version3, version4);
            Assert.True(system.WasComponentModified);
        }

        internal class GetVersionSystem<T> : SystemBase where T : struct
        {
            public EntityQuery<T> Query = null;

            public bool WasComponentModified { get; private set; } = false;
            public bool UseReadOnly = false;

            public override void OnUpdate(float deltaTime)
            {
                foreach (var entity in Query)
                {
                    if (UseReadOnly)
                    {
                        ref readonly var foo = ref entity.GetReadOnlyComponent<SampleStructs.FooData>();
                    }
                    else
                    {
                        ref var foo = ref entity.GetComponent<SampleStructs.FooData>();
                    }

                    var version = entity.GetComponentVersion<SampleStructs.FooData>();

                    WasComponentModified = DidChange(version);
                }
            }
        }
    }
}
