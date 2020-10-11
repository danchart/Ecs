using System.Threading;
using Xunit;

namespace Ecs.Core.Tests
{
    public class AppendOnlyListTests
    {
        [Fact]
        public void Test()
        {
            var list = new AppendOnlyList<SampleStructs.Foo>(2);

            list.Add(new SampleStructs.Foo { x = 1 });
            list.Add(new SampleStructs.Foo { x = 2 });
            list.Add(new SampleStructs.Foo { x = 3 });
            list.Add(new SampleStructs.Foo { x = 4 });

            Assert.Equal(4, list.Count);
            Assert.Equal(1, list.Items[0].x);
            Assert.Equal(2, list.Items[1].x);
            Assert.Equal(3, list.Items[2].x);
            Assert.Equal(4, list.Items[3].x);
        }
    }
}
