using System.Net;
using System.Text;
using System.Threading;
using Test.Common;
using Xunit;

namespace Game.Networking.Core.Tests
{
    public class UdpSocketTests
    {
        [Fact]
        public void ClientServerSendReceive()
        {
            var logger = new TestLogger();
            var serverBuffer = new ReceiveBuffer(maxPacketSize: 64, packetQueueCapacity: 4);

            var server = new UdpSocketServer(logger, serverBuffer);
            server.Start(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000));

            var clientBuffer = new ReceiveBuffer(maxPacketSize: 64, packetQueueCapacity: 4);
            var client = new UdpSocketClient(logger, clientBuffer);
            client.Start(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000));

            // Simulate TCP style SYN > SYN-ACK > ACK handshake...

            // Client->Server SYN

            client.Send(Encoding.ASCII.GetBytes("SYN"));
            client.Send(Encoding.ASCII.GetBytes("SYN 2")); // Just to test multiple packets

            WaitForReceive(serverBuffer, 2);

            Assert.Equal(2, serverBuffer.QueueCount);

            byte[] data;
            int offset;
            int size;
            IPEndPoint clientIpEndPoint;
            serverBuffer.GetReadBufferData(out data, out offset, out size);
            serverBuffer.GetFromEndPoint(out clientIpEndPoint);
            serverBuffer.NextRead();
            var text0 = Encoding.ASCII.GetString(data, offset, size);

            serverBuffer.GetReadBufferData(out data, out offset, out size);
            serverBuffer.NextRead();
            var text1 = Encoding.ASCII.GetString(data, offset, size);

            Assert.Equal(0, serverBuffer.QueueCount);

            Assert.True((text0 == "SYN" && text1 == "SYN 2") ||
                (text0 == "SYN 2" && text1 == "SYN"));

            // Server->Client SYN-ACK

            server.SendTo(Encoding.ASCII.GetBytes("SYN-ACK"), clientIpEndPoint);

            WaitForReceive(clientBuffer, 1);

            clientBuffer.GetReadBufferData(out data, out offset, out size);
            clientBuffer.NextRead();
            var syncAckText = Encoding.ASCII.GetString(data, offset, size);

            Assert.Equal("SYN-ACK", syncAckText);

            // Client->Server ACK

            client.Send(Encoding.ASCII.GetBytes("ACK"));

            WaitForReceive(serverBuffer, 1);

            serverBuffer.GetReadBufferData(out data, out offset, out size);
            serverBuffer.NextRead();
            var ackText = Encoding.ASCII.GetString(data, offset, size);

            Assert.Equal("ACK", ackText);
        }

        private static void WaitForReceive(ReceiveBuffer serverBuffer, int queueCount)
        {
            for (int countdown = 10; countdown >= 0 && serverBuffer.QueueCount != queueCount; countdown--)
            {
                Thread.Sleep(1);
            }
        }
    }
}
