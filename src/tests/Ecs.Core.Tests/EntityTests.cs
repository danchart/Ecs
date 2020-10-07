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
            entity1.GetComponent<SampleStructs.Foo>();
            var entity2 = world.NewEntity();
            entity2.GetComponent<SampleStructs.Bar>();

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

            ref var foo = ref entity.GetComponent<SampleStructs.Foo>();
            // Resize should happen here.
            ref var bar = ref entity.GetComponent<SampleStructs.Bar>();
        }

        [Fact]
        public void ComponentRef()
        {
            var world = Helpers.NewWorld();

            var entity1 = world.NewEntity();
            ref var compFoo = ref entity1.GetComponent<SampleStructs.Foo>();
            compFoo.x = 1;

            var compRefFoo = entity1.Reference<SampleStructs.Foo>();
            ref var compFooFromRef = ref compRefFoo.Unref();

            compFooFromRef.x = 2;

            Assert.Equal(compFoo.x, compFooFromRef.x);
        }

        [Fact]
        public void RemoveComponent()
        {
            var world = Helpers.NewWorld();

            var entity1 = world.NewEntity();
            ref var compFoo = ref entity1.GetComponent<SampleStructs.Foo>();
            compFoo.x = 1;
            ref var compBar = ref entity1.GetComponent<SampleStructs.Bar>();

            Assert.True(entity1.HasComponent<SampleStructs.Foo>());

            entity1.RemoveComponent<SampleStructs.Foo>();

            Assert.False(entity1.HasComponent<SampleStructs.Foo>());
        }

        [Fact]
        public void ReplaceComponent()
        {
            var world = Helpers.NewWorld();
            var entity = world.NewEntity();
            ref var value = ref entity.GetComponent<SampleStructs.Foo>();
            value.x = 1;
            value.text = "helo";

            ref var value2 = ref entity.GetComponent<SampleStructs.Foo>();

            Assert.Equal(value, value2);
            Assert.Equal(1, value.x);
            Assert.Equal("helo", value.text);

            var newValue = new SampleStructs.Foo
            {
                x = 7,
                text = "bye"
            };

            entity.ReplaceComponent(newValue);

            ref var value3 = ref entity.GetComponent<SampleStructs.Foo>();

            Assert.Equal(value, value3);
            Assert.Equal(7, value.x);
            Assert.Equal("bye", value.text);
        }

        [Fact]
        public void GetVersion()
        {
            var systems = new Systems(Helpers.NewWorld());
            var system = new GetVersionSystem<SampleStructs.Foo>();
            systems
                .Add(system)
                .Create();

            var entity = systems.World.NewEntity();
            entity.GetComponent<SampleStructs.Foo>();

            var version1 = entity.GetComponentVersion<SampleStructs.Foo>();

            systems.Run(1);

            var version2 = entity.GetComponentVersion<SampleStructs.Foo>();

            Assert.NotEqual(version1, version2);
            Assert.True(system.WasComponentModified);

            system.UseReadOnly = true;
            systems.Run(1);
            var version3 = entity.GetComponentVersion<SampleStructs.Foo>();

            Assert.Equal(version2, version3);
            Assert.False(system.WasComponentModified);

            system.UseReadOnly = false;
            systems.Run(1);
            var version4 = entity.GetComponentVersion<SampleStructs.Foo>();

            Assert.NotEqual(version3, version4);
            Assert.True(system.WasComponentModified);
        }

        [Fact]
        public void GetComponentAndVersion()
        {
            var systems = new Systems(Helpers.NewWorld());
            var system = new GetComponentAndVersionSystem<SampleStructs.Foo>();
            systems
                .Add(system)
                .Create();

            var entity = systems.World.NewEntity();
            entity.GetComponent<SampleStructs.Foo>();

            system.UseReadOnly = false;

            systems.Run(1);

            Assert.True(system.WasComponentModified);

            system.UseReadOnly = true;
            systems.Run(1);

            Assert.False(system.WasComponentModified);
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
                        ref readonly var foo = ref entity.GetReadOnlyComponent<SampleStructs.Foo>();
                    }
                    else
                    {
                        ref var foo = ref entity.GetComponent<SampleStructs.Foo>();
                    }

                    var version = entity.GetComponentVersion<SampleStructs.Foo>();
                    Version version2;
                    entity.GetComponentAndVersion<SampleStructs.Foo>(out version2);

                    WasComponentModified = IsChanged(version);
                }
            }
        }

        internal class GetComponentAndVersionSystem<T> : SystemBase where T : struct
        {
            public EntityQuery<T> Query = null;

            public bool WasComponentModified { get; private set; } = false;
            public bool UseReadOnly = false;

            public Version Version;

            public override void OnUpdate(float deltaTime)
            {
                foreach (var entity in Query)
                {
                    if (UseReadOnly)
                    {
                        ref readonly var foo = ref entity.GetReadonlyComponentAndVersion<SampleStructs.Foo>(out Version);
                    }
                    else
                    {
                        ref var foo = ref entity.GetComponentAndVersion<SampleStructs.Foo>(out Version);
                    }

                    WasComponentModified = IsChanged(Version);
                }
            }
        }
    }
}
