using Xunit;

namespace Ecs.Core.Tests
{
    public class ComponentTests
    {
        [Fact]
        public void ComponentReference()
        {
            var world = Helpers.NewWorld();

            var entity = world.NewEntity();

            // Add 2 components

            ref var foo = ref entity.GetComponent<SampleStructs.Foo>();
            foo.x = 1;
            foo.y = 2;

            var fooRef = entity.Reference<SampleStructs.Foo>();
            ref var fooFromRef = ref fooRef.Unref();

            fooFromRef.x = -1;
            fooFromRef.y = -2;

            Assert.Equal(foo, fooFromRef);
            Assert.Equal(-1, foo.x);
            Assert.Equal(-2, foo.y);
        }
    }
}
