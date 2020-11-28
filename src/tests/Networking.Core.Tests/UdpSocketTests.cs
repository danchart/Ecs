using System.IO;
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
            const int MaxPacketSize = 64;

            var encryptor = new XorPacketEncryptor();
            var logger = new TestLogger();
            var serverPacketBuffer = new PacketBuffer<ServerPacket>(encryptor, size: 4);

            var server = new ServerUdpSocket<ServerPacket>(logger, serverPacketBuffer, MaxPacketSize);
            server.Start(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000));

            var clientPacketBuffer = new PacketBuffer<ClientPacket>(encryptor, size: 4);
            var client = new ClientUdpSocket<ClientPacket>(logger, clientPacketBuffer, MaxPacketSize);
            client.Start(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000));







            // Simulate TCP style SYN > SYN-ACK > ACK handshake...

            // Client->Server SYN

            client.Send(Encoding.ASCII.GetBytes("SYN"));
            client.Send(Encoding.ASCII.GetBytes("SYN 2")); // Just to test multiple packets

            WaitForReceive(serverPacketBuffer, 2);

            Assert.Equal(2, serverPacketBuffer.Count);

            byte[] data;
            int offset;
            int size;
            IPEndPoint clientIpEndPoint;
            serverPacketBuffer.BeginRead(out data, out offset, out size);
            serverPacketBuffer.GetEndPoint(out clientIpEndPoint);
            serverPacketBuffer.EndRead();
            var text0 = Encoding.ASCII.GetString(data, offset, size);

            serverPacketBuffer.BeginRead(out data, out offset, out size);
            serverPacketBuffer.EndRead();
            var text1 = Encoding.ASCII.GetString(data, offset, size);

            Assert.Equal(0, serverPacketBuffer.Count);

            Assert.True((text0 == "SYN" && text1 == "SYN 2") ||
                (text0 == "SYN 2" && text1 == "SYN"));

            // Server->Client SYN-ACK

            server.SendTo(Encoding.ASCII.GetBytes("SYN-ACK"), clientIpEndPoint);

            WaitForReceive(clientPacketBuffer, 1);

            clientPacketBuffer.BeginRead(out data, out offset, out size);
            clientPacketBuffer.EndRead();
            var syncAckText = Encoding.ASCII.GetString(data, offset, size);

            Assert.Equal("SYN-ACK", syncAckText);

            // Client->Server ACK

            client.Send(Encoding.ASCII.GetBytes("ACK"));

            WaitForReceive(serverPacketBuffer, 1);

            serverPacketBuffer.BeginRead(out data, out offset, out size);
            serverPacketBuffer.EndRead();
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

        private struct ServerPacket : IPacketSerialization
        {
            public int a, b, c;

            public bool Deserialize(Stream stream)
            {
                stream.PacketReadInt(out this.a);
                stream.PacketReadInt(out this.b);
                stream.PacketReadInt(out this.c);

                return true;
            }

            public int Serialize(Stream stream)
            {
                return
                    stream.PacketWriteInt(this.a)
                    + stream.PacketWriteInt(this.b)
                    + stream.PacketWriteInt(this.c);
            }
        }

        private struct ClientPacket : IPacketSerialization
        {
            public float xAxis;
            public float yAxis;

            public bool Deserialize(Stream stream)
            {
                stream.PacketReadFloat(out xAxis);
                stream.PacketReadFloat(out yAxis);

                return true;
            }

            public int Serialize(Stream stream)
            {
                return
                    stream.PacketWriteFloat(this.xAxis)
                    + stream.PacketWriteFloat(this.yAxis);
            }
        }
    }
}
