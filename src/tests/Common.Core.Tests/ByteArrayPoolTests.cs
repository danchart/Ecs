using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Common.Core.Tests
{
    public class ByteArrayPoolTests
    {
        [Fact]
        public void Tests()
        {
            var pool = new ByteArrayPool(arraySize: 16, poolCapacity: 2);

            var idx1 = pool.New();

            Assert.Equal(1, pool.Count);

            var buffer1 = pool.GetBuffer(idx1);

            Assert.Equal(16, buffer1.Length);

            var idx2 = pool.New();

            Assert.Equal(2, pool.Count);

            var buffer2 = pool.GetBuffer(idx2);

            Assert.Equal(16, buffer2.Length);

            var idx3 = pool.New(); // out of capacity

            Assert.Equal(2, pool.Count);

            buffer1[0] = 1;
            buffer1[1] = 2;

            buffer2[0] = 3;
            buffer2[1] = 4;

            Assert.Equal(1, buffer1[0]);
            Assert.Equal(2, buffer1[1]);
            Assert.Equal(3, buffer2[0]);
            Assert.Equal(4, buffer2[1]);

            pool.Free(idx1);

            Assert.Equal(1, pool.Count);

            var idx4 = pool.New();

            Assert.Equal(2, pool.Count);

            var buffer4 = pool.GetBuffer(idx4);

            buffer4[0] = 5;
            buffer4[1] = 6;

            Assert.Equal(3, buffer2[0]);
            Assert.Equal(4, buffer2[1]);
            Assert.Equal(5, buffer4[0]);
            Assert.Equal(6, buffer4[1]);

            pool.Free(idx2);
            pool.Free(idx4);

            Assert.Equal(0, pool.Count);
        }
    }
}
