using System.Threading;
using Xunit;

namespace Ecs.Core.Tests.Collections
{
    public class AppendOnlyListTests
    {
        [Fact]
        public void Test()
        {
            var list = new AppendOnlyList<SampleStructs.Foo>(2);

            list.Add(new SampleStructs.Foo { });
            list.Add(new SampleStructs.Foo { });
            list.Add(new SampleStructs.Foo { });
            list.Add(new SampleStructs.Foo { text = "helo" });

            Assert.Equal(4, list.Count);
            Assert.Equal("helo", list.Items[3].text);
        }
    }
}
