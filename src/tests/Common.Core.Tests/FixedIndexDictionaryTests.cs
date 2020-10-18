using System;
using Xunit;

namespace Common.Core.Tests
{
    public class FixedIndexDictionaryTests
    {
        [Fact]
        public void Test()
        {
            var dict = new FixedIndexDictionary<int>(size: 10);

            dict[1] = 1;
            dict[7] = 7;

            Assert.False(dict.ContainsKey(0));
            Assert.True(dict.ContainsKey(1));
            Assert.Equal(1, dict[1]);
            Assert.True(dict.ContainsKey(7));
            Assert.Equal(7, dict[7]);
            Assert.False(dict.ContainsKey(9));

            dict.Clear();

            Assert.False(dict.ContainsKey(0));
            Assert.False(dict.ContainsKey(1));

            dict[0] = 0;
            dict[1] = -1;

            Assert.Equal(0, dict[0]);
            Assert.Equal(-1, dict[1]);

            Assert.Throws<IndexOutOfRangeException>(() => dict[11]);
        }
    }
}
