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
#if NEVER
        [Fact]
        public void ClientServerSendReceive()
        {
            const int MaxPacketSize = 64;

            var encryptor = new XorPacketEncryptor();
            var logger = new TestLogger();
            var serverPacketBuffer = new PacketBuffer<ServerPacket>(size: 4);

            var serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000);

            var serverSocket = new ServerUdpSocket<ServerPacket>(logger, serverPacketBuffer, encryptor, MaxPacketSize);
            serverSocket.Start(serverEndPoint);

            var clientPacketBuffer = new PacketBuffer<ClientPacket>(size: 4);
            var client = new ClientUdpSocket<ClientPacket>(logger, clientPacketBuffer, encryptor, MaxPacketSize);
            client.Start(serverEndPoint);


            // Test simulation UDP loop:
            //  - client waits for packet
            //  - server sends packet
            //  - client sends input + ack
            //  - server recieves ack

            var serverState = new ServerState
            {
                Sequence = 1,
                Socket = serverSocket,
                PacketBuffer = serverPacketBuffer,

                ClientEndPoint = (IPEndPoint) client.LocalEndPoint,
            };

            var timer = new Timer(
                new TimerCallback(ServerCallback),
                serverState,
                0,
                16);




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

            serverSocket.SendTo(Encoding.ASCII.GetBytes("SYN-ACK"), clientIpEndPoint);

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

        private void ServerCallback(object obj)
        {
            var state = (ServerState)obj;

            state.Socket.SendTo(
                new PacketEnvelope<ServerPacket>
                {
                    Header = new PacketEnvelopeHeader
                    {
                        Sequence = state.Sequence++,
                        Ack = state.PacketBuffer.Ack
                    }
                },
                state.ClientEndPoint);
        }


        private static void WaitForReceive(ReceiveBuffer serverBuffer, int queueCount)
        {
            for (int countdown = 10; countdown >= 0 && serverBuffer.Count != queueCount; countdown--)
            {
                Thread.Sleep(1);
            }
        }

        private class ServerState
        {
            public ushort Sequence;
            public ServerUdpSocket<ServerPacket> Socket;
            public PacketBuffer<ServerPacket> PacketBuffer;

            public IPEndPoint ClientEndPoint;
        }

        private struct ServerPacket : IPacketSerialization
        {
            // Frame?

            public float x, y;

            public bool Deserialize(Stream stream)
            {
                stream.PacketReadFloat(out this.x);
                stream.PacketReadFloat(out this.y);

                return true;
            }

            public int Serialize(Stream stream)
            {
                return
                    stream.PacketWriteFloat(this.x)
                    + stream.PacketWriteFloat(this.y);
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
#endif
    }
}
