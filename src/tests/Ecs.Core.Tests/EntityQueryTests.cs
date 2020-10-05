using System;
using Xunit;

namespace Ecs.Core.Tests
{
    public class EntityQueryTests
    {
        [Fact]
        public void Resize()
        {
            var world = Helpers.NewWorld();

            var entity1 = world.NewEntity();

            ref var e1CompFoo = ref entity1.GetComponent<SampleStructs.FooData>();
            ref var e1CompBar = ref entity1.GetComponent<SampleStructs.BarData>();

            var entity2 = world.NewEntity();

            ref var e2CompFoo = ref entity2.GetComponent<SampleStructs.FooData>();

            var query = new EntityQuery(
                new Type[] 
                {
                    typeof(SampleStructs.FooData),
                    typeof(SampleStructs.BarData),
                });

            int i = 0;
        }
    }
}
