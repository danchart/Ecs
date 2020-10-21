using System.Text;
using System.Threading;
using Xunit;

namespace Game.Networking.Core.Tests
{
    public class UdpSocketTests
    {
        [Fact]
        public void ClientServerSendReceive()
        {
            var serverBuffer = new ReceiveBuffer(maxPacketSize: 64, packetQueueCapacity: 4);

            var server = new UdpSocket(serverBuffer);
            server.Server("127.0.0.1", 27000);

            var clientBuffer = new ReceiveBuffer(maxPacketSize: 64, packetQueueCapacity: 4);
            var client = new UdpSocket(clientBuffer);
            client.Client("127.0.0.1", 27000);

            client.Send(Encoding.ASCII.GetBytes("hello"));
            client.Send(Encoding.ASCII.GetBytes("iam john"));

            for (int countdown= 10; countdown >= 0 && serverBuffer.QueueCount != 2; countdown--)
            {
                Thread.Sleep(1);
            }

            Assert.Equal(2, serverBuffer.QueueCount);

            byte[] data;
            int offset;
            int size;

            serverBuffer.GetReadBufferData(out data, out offset, out size);
            var text = Encoding.ASCII.GetString(data, offset, size);

            Assert.Equal("hello", text);

            serverBuffer.NextRead();
            serverBuffer.GetReadBufferData(out data, out offset, out size);
            text = Encoding.ASCII.GetString(data, offset, size);

            Assert.Equal("iam john", text);
        }
    }
}
