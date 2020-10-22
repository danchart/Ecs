using System.Net;
using Xunit;

namespace Game.Networking.Core.Tests
{
    public class ReceiveBufferTests
    {
        [Fact]
        public void Test()
        {
            var buffer = new ReceiveBuffer(maxPacketSize: 16, packetQueueCapacity: 4);

            Assert.Equal(0, buffer.QueueCount);

            byte[] data;
            int offset;
            int size;
            var ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Assert.True(buffer.GetWriteBufferData(out data, out offset, out size));

            data[offset] = 1;
            data[offset+1] = 2;
            buffer.NextWrite(2, ipEndPoint);

            Assert.Equal(1, buffer.QueueCount);

            Assert.True(buffer.GetWriteBufferData(out data, out offset, out size));
            data[offset] = 3;
            data[offset + 1] = 4;
            buffer.NextWrite(2, ipEndPoint);

            Assert.Equal(2, buffer.QueueCount);

            buffer.NextWrite(0, ipEndPoint);
            Assert.Equal(3, buffer.QueueCount);

            buffer.NextWrite(0, ipEndPoint);
            Assert.Equal(4, buffer.QueueCount);

            // Overflow
            Assert.False(buffer.GetWriteBufferData(out data, out offset, out size));
            Assert.Equal(4, buffer.QueueCount);

            Assert.True(buffer.GetReadBufferData(out data, out offset, out size));
            Assert.Equal(2, size);
            Assert.Equal(1, data[offset]);
            Assert.Equal(2, data[offset+1]);

            buffer.NextRead();

            Assert.Equal(3, buffer.QueueCount);

            Assert.True(buffer.GetReadBufferData(out data, out offset, out size));
            Assert.Equal(2, size);
            Assert.Equal(3, data[offset]);
            Assert.Equal(4, data[offset + 1]);

            buffer.NextRead();

            Assert.Equal(2, buffer.QueueCount);

            Assert.True(buffer.GetReadBufferData(out data, out offset, out size));
            Assert.Equal(0, size);

            buffer.NextRead();

            Assert.Equal(1, buffer.QueueCount);

            Assert.True(buffer.GetReadBufferData(out data, out offset, out size));
            Assert.Equal(0, size);

            buffer.NextRead();

            Assert.Equal(0, buffer.QueueCount);
            Assert.False(buffer.GetReadBufferData(out data, out offset, out size));
        }
    }
}
