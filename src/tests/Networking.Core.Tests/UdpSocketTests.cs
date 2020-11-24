using Common.Core;
using System.Net;
using System.Text;
using System.Threading;
using Test.Common;
using Xunit;

namespace Networking.Core.Tests
{
    public class UdpSocketTests
    {
        [Fact]
        public void ClientServerSendReceive()
        {
            var logger = new TestLogger();
            var serverBuffer = new ReceiveBuffer(maxPacketSize: 64, packetQueueCapacity: 4);

            var server = new ServerUdpSocket(logger, serverBuffer);
            server.Start(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000));

            var clientBuffer = new ReceiveBuffer(maxPacketSize: 64, packetQueueCapacity: 4);
            var client = new ClientUdpSocket(logger, clientBuffer);
            client.Start(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000));

            // Simulate TCP style SYN > SYN-ACK > ACK handshake...

            // Client->Server SYN

            client.Send(Encoding.ASCII.GetBytes("SYN"));
            client.Send(Encoding.ASCII.GetBytes("SYN 2")); // Just to test multiple packets

            WaitForReceive(serverBuffer, 2);

            Assert.Equal(2, serverBuffer.Count);

            byte[] data;
            int offset;
            int size;
            IPEndPoint clientIpEndPoint;
            serverBuffer.BeginRead(out data, out offset, out size);
            serverBuffer.GetEndPoint(out clientIpEndPoint);
            serverBuffer.EndRead();
            var text0 = Encoding.ASCII.GetString(data, offset, size);

            serverBuffer.BeginRead(out data, out offset, out size);
            serverBuffer.EndRead();
            var text1 = Encoding.ASCII.GetString(data, offset, size);

            Assert.Equal(0, serverBuffer.Count);

            Assert.True((text0 == "SYN" && text1 == "SYN 2") ||
                (text0 == "SYN 2" && text1 == "SYN"));

            // Server->Client SYN-ACK

            server.SendTo(Encoding.ASCII.GetBytes("SYN-ACK"), clientIpEndPoint);

            WaitForReceive(clientBuffer, 1);

            clientBuffer.BeginRead(out data, out offset, out size);
            clientBuffer.EndRead();
            var syncAckText = Encoding.ASCII.GetString(data, offset, size);

            Assert.Equal("SYN-ACK", syncAckText);

            // Client->Server ACK

            client.Send(Encoding.ASCII.GetBytes("ACK"));

            WaitForReceive(serverBuffer, 1);

            serverBuffer.BeginRead(out data, out offset, out size);
            serverBuffer.EndRead();
            var ackText = Encoding.ASCII.GetString(data, offset, size);

            Assert.Equal("ACK", ackText);
        }

        private static void WaitForReceive(ReceiveBuffer serverBuffer, int queueCount)
        {
            for (int countdown = 10; countdown >= 0 && serverBuffer.Count != queueCount; countdown--)
            {
                Thread.Sleep(1);
            }
        }
    }
}
