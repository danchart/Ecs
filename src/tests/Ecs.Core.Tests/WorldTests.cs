using Xunit;

namespace Ecs.Core.Tests
{
    public class WorldTests
    {
        [Fact]
        public void EndToEnd()
        {
            var world = new World(EcsConfig.Default);

            var entity1 = world.NewEntity();
            var entity2 = world.NewEntity();

            ref var e1CompFoo = ref entity1.GetComponent<SampleStructs.FooData>();
            ref var e1CompFoo2 = ref entity1.GetComponent<SampleStructs.FooData>();

            ref var e1CompBar = ref entity1.GetComponent<SampleStructs.BarData>();
            ref var e1CompBar2 = ref entity1.GetComponent<SampleStructs.BarData>();

            ref var e2CompFoo = ref entity2.GetComponent<SampleStructs.FooData>();
            ref var e2CompBar = ref entity2.GetComponent<SampleStructs.BarData>();

            e1CompFoo.x = 1;
            e1CompFoo.y = 2;
            e1CompFoo.text = "helo";

            e1CompBar.a = 5;
            e1CompBar.b = 7;
            e1CompBar.c = true;

            e2CompFoo.x = 3;
            e2CompFoo.y = 4;
            e2CompFoo.text = "bye";

            e2CompBar.a = 13;
            e2CompBar.b = 15;
            e2CompBar.c = false;

            Assert.Equal(1, e1CompFoo.x);
            Assert.Equal(2, e1CompFoo.y);
            Assert.Equal("helo", e1CompFoo.text);

            Assert.Equal(e1CompFoo, e1CompFoo2);
            Assert.Equal(e1CompBar, e1CompBar2);

            Assert.Equal(5, e1CompBar.a);
            Assert.Equal(7, e1CompBar.b);
            Assert.True(e1CompBar.c);

            Assert.Equal(3, e2CompFoo.x);
            Assert.Equal(4, e2CompFoo.y);
            Assert.Equal("bye", e2CompFoo.text);

            Assert.Equal(13, e2CompBar.a);
            Assert.Equal(15, e2CompBar.b);
            Assert.False(e2CompBar.c);

            Assert.True(entity1.HasComponent<SampleStructs.FooData>());
            Assert.True(entity1.HasComponent<SampleStructs.BarData>());
        }
    }
}
