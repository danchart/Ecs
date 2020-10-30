using Database.Server;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Test.Common;
using Xunit;

namespace Networking.Core.Tests
{
    public class TcpSocketTests
    {
        [Fact]
        public void ClientServerSendReceive()
        {
            var logger = new TestLogger();
            var serverBuffer = new TcpReceiveBuffer(maxPacketSize: 64, packetQueueCapacity: 4);

            var serverEp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27005);

            var server = new TcpSocketListener(logger, serverBuffer);
            server.Start(serverEp);

            var serverThread = new Thread(server.Receive);
            serverThread.Start();

            var client = new TcpSocketClient(logger);

            client.Connect(serverEp.Address.ToString(), serverEp.Port);

            var bytesSend = Encoding.ASCII.GetBytes("Hello, world");
            client.Send(bytesSend, 0, bytesSend.Length);

            WaitForReceive(serverBuffer, 1, server);

            Assert.Equal(1, serverBuffer.Count);

            byte[] data = new byte[256];
            int offset, count;
            Assert.True(serverBuffer.GetReadData(out data, out offset, out count));

            var receivedStr = Encoding.ASCII.GetString(data, offset, count);
            Assert.Equal("Hello, world", receivedStr);

            serverBuffer.GetClient(out TcpClient tcpClient);

            var stream = tcpClient.GetStream();

            var bytesSend2 = Encoding.ASCII.GetBytes("I'm server");
            stream.Write(bytesSend2, 0, bytesSend2.Length);

            {
                var bs = Encoding.ASCII.GetBytes("This is packet # 2");
                stream.Write(bs, 0, bs.Length);
            }

            Thread.Sleep(100);

            client.Read(data, 0, data.Length, out count);

            var receivedStr2 = Encoding.ASCII.GetString(data, offset, count);

            Assert.Equal("I'm server", receivedStr2);

            server.Stop();
        }

        private static void WaitForReceive(TcpReceiveBuffer serverBuffer, int queueCount, TcpSocketListener server)
        {
            for (int countdown = 10; countdown >= 0 && serverBuffer.Count != queueCount; countdown--)
            {
                Thread.Sleep(1);
            }
        }
    }
}
