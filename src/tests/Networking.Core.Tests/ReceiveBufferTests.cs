using System.Net;
using Xunit;

namespace Networking.Core.Tests
{
    public class ReceiveBufferTests
    {
        [Fact]
        public void Test()
        {
            var buffer = new ReceiveBuffer(maxPacketSize: 16, packetQueueCapacity: 4);

            Assert.Equal(0, buffer.Count);

            byte[] data;
            int offset;
            int size;
            var ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Assert.True(buffer.BeginWrite(out data, out offset, out size));

            data[offset] = 1;
            data[offset+1] = 2;
            buffer.EndWrite(2, ipEndPoint);

            Assert.Equal(1, buffer.Count);

            Assert.True(buffer.BeginWrite(out data, out offset, out size));
            data[offset] = 3;
            data[offset + 1] = 4;
            buffer.EndWrite(2, ipEndPoint);

            Assert.Equal(2, buffer.Count);

            buffer.EndWrite(0, ipEndPoint);
            Assert.Equal(3, buffer.Count);

            buffer.EndWrite(0, ipEndPoint);
            Assert.Equal(4, buffer.Count);

            // Overflow
            Assert.False(buffer.BeginWrite(out data, out offset, out size));
            Assert.Equal(4, buffer.Count);

            Assert.True(buffer.BeginRead(out data, out offset, out size));
            Assert.Equal(2, size);
            Assert.Equal(1, data[offset]);
            Assert.Equal(2, data[offset+1]);

            buffer.EndRead();

            Assert.Equal(3, buffer.Count);

            Assert.True(buffer.BeginRead(out data, out offset, out size));
            Assert.Equal(2, size);
            Assert.Equal(3, data[offset]);
            Assert.Equal(4, data[offset + 1]);

            buffer.EndRead();

            Assert.Equal(2, buffer.Count);

            Assert.True(buffer.BeginRead(out data, out offset, out size));
            Assert.Equal(0, size);

            buffer.EndRead();

            Assert.Equal(1, buffer.Count);

            Assert.True(buffer.BeginRead(out data, out offset, out size));
            Assert.Equal(0, size);

            buffer.EndRead();

            Assert.Equal(0, buffer.Count);
            Assert.False(buffer.BeginRead(out data, out offset, out size));
        }
    }
}
