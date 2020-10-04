using Xunit;

namespace Ecs.Core.Tests
{
    public class WorldTests
    {
        [Fact]
        public void Test()
        {
            var world = new World();

            var entity = world.NewEntity();

            ref var compFoo = ref entity.GetComponent<FooData>();
            ref var compBar = ref entity.GetComponent<BarData>();

            compFoo.x = 1;
            compFoo.y = 2;
            compFoo.text = "helo";

            compBar.a = 5;
            compBar.b = 7;
            compBar.c = true;

            int i = 0;
        }

        private struct FooData
        {
            public int x, y;
            public string text;
        }

        private struct BarData
        {
            public int a, b;
            public bool c;
        }
    }
}
