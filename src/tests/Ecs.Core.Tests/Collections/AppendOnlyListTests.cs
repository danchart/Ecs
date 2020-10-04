using System.Threading;
using Xunit;

namespace Ecs.Core.Tests.Collections
{
    public class AppendOnlyListTests
    {
        [Fact]
        public void Test()
        {
            var list = new AppendOnlyList<SampleStructs.FooData>(2);

            list.Add(new SampleStructs.FooData { });
            list.Add(new SampleStructs.FooData { });
            list.Add(new SampleStructs.FooData { });
            list.Add(new SampleStructs.FooData { text = "helo" });

            Assert.Equal(4, list.Count);
            Assert.Equal("helo", list.Items[3].text);
        }
    }
}
