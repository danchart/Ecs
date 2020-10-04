using System.Collections.Generic;
using Xunit;

namespace Ecs.Core.Tests
{
    public class EntityTests
    {
        [Fact]
        public void Equality()
        {
            var world = new World(EcsConfig.Default);

            var entity1 = world.NewEntity();
            var entity2 = world.NewEntity();

            var set = new HashSet<Entity>
            {
                entity1, entity2
            };

            Assert.NotEqual(entity1, entity2);
            Assert.True(entity1 != entity2);
            Assert.True(entity1 == entity1);
            Assert.True(entity1.Equals(entity1));
            Assert.False(entity1.Equals(entity2));
            Assert.Equal(2, set.Count);
        }

        /// <summary>
        /// Validate entity component array resize
        /// </summary>
        [Fact]
        public void Resize()
        {
            var config = EcsConfig.Default;
            // Initial array count of 1
            config.InitialEntityComponentCount = 1;

            var world = new World(config);

            var entity = world.NewEntity();

            // Add 2 components

            ref var foo = ref entity.GetComponent<SampleStructs.FooData>();
            // Resize should happen here.
            ref var bar = ref entity.GetComponent<SampleStructs.BarData>();
        }
    }
}
