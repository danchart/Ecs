using Xunit;

namespace Ecs.Core.Tests
{
    public class ComponentTests
    {
        [Fact]
        public void ComponentReference()
        {
            var world = new World(EcsConfig.Default);

            var entity = world.NewEntity();

            // Add 2 components

            ref var foo = ref entity.GetComponent<SampleStructs.FooData>();
            foo.x = 1;
            foo.text = "helo";

            var fooRef = entity.Reference<SampleStructs.FooData>();
            ref var fooFromRef = ref fooRef.Unref();

            fooFromRef.x = 2;
            fooFromRef.text = "bye";

            Assert.Equal(foo, fooFromRef);
            Assert.Equal(2, foo.x);
            Assert.Equal("bye", foo.text);
        }
    }
}
