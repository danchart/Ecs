using Xunit;

namespace Ecs.Core.Tests
{
    public class WorldTests
    {
        [Fact]
        public void Test()
        {
            var world = new World(EcsConfig.Default);

            var entity = world.NewEntity();

            ref var compFoo = ref entity.GetComponent<SampleStructs.FooData>();
            ref var compBar = ref entity.GetComponent<SampleStructs.BarData>();

            compFoo.x = 1;
            compFoo.y = 2;
            compFoo.text = "helo";

            compBar.a = 5;
            compBar.b = 7;
            compBar.c = true;

            int i = 0;
        }
    }
}
