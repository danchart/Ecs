using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace Common.Core.Tests
{
    public class RefDictionaryTests
    {
        [Fact]
        public void Test()
        {
            var dict = new RefDictionary<int, DataStruct>(capacity: 1);

            Assert.Equal(0, dict.Count);

            dict.Add(0, new DataStruct { a = 0 });
            dict.Add(1, new DataStruct { a = 1 });
            dict.Add(2, new DataStruct { a = 2 });

            Assert.Equal(3, dict.Count);
            Assert.Equal(0, dict[0].a);
            Assert.Equal(1, dict[1].a);
            Assert.Equal(2, dict[2].a);

            // Enumerator
            int count = 0;
            foreach (var item in dict)
            {
                Assert.Equal(count++, item.a);
            }

            dict.Remove(1);

            Assert.Equal(2, dict.Count);
            Assert.True(dict.ContainsKey(1));
            Assert.Throws<KeyNotFoundException>(() => dict[1]);

            dict.Clear();

            Assert.Equal(0, dict.Count);

            dict.Add(3, new DataStruct { a = 3 });
            dict.Add(4, new DataStruct { a = 4 });

            Assert.Equal(2, dict.Count);
            Assert.Equal(3, dict[3].a);
            Assert.Equal(4, dict[4].a);

            ref var i3 = ref dict[4];

            i3.a = -3;

            Assert.Equal(-3, dict[3].a);
        }

        private struct DataStruct
        {
            public int a, b;
        }
    }
}
