using Common.Core;
using Database.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Test.Common;
using Xunit;

namespace Networking.Core.Tests
{
    public class TcpSocketTests
    {
        private static async Task<byte[]> ProcessAsync(byte[] data, CancellationToken token)
        {
            //var requestMessage = MessagePackSerializer.Deserialize<SampleDataMsgPack>(
            //    new ReadOnlyMemory<byte>(data, 0, data.Length));

            //var responseMessage = new SampleDataMsgPack
            //{
            //    ClientId = requestMessage.ClientId,
            //    MyInt = requestMessage.MyInt,
            //    MyString = "The quick brown fox jumped over the lazy dogs.",
            //};

            //return MessagePackSerializer.Serialize(responseMessage);
        }

        [Fact]
        private async void TestAsync()
        {
            // Testing constants:

            const int MaxPacketSize = 256;
            const int PacketQueueCapacity = 256;

            const int TestRequestCount = 100000;
            const int ClientCount = 5;
            const int ConcurrentRequestCount = 100;

            var logger = new TestLogger();
            var random = new Random();

            // Start TCP server.

            //IPAddress.Parse("127.0.0.1")
            var addressList = Dns.GetHostEntry("localhost").AddressList;
            var ipAddress = addressList.First();
            var serverEndpoint = new IPEndPoint(ipAddress, 27005);

            var server = new TcpServer(logger, clientCapacity: 2, maxPacketSize: MaxPacketSize, packetQueueDepth: PacketQueueCapacity);

            server.Start(serverEndpoint, ProcessAsync);

            // TCP clients connecto to server.

            var clients = new TcpSocketClient[ClientCount];

            for (int i = 0; i < clients.Length; i++)
            {
                clients[i] = new TcpSocketClient(logger, maxPacketSize: MaxPacketSize, packetQueueCapacity: PacketQueueCapacity);
                clients[i].Connect(new IPAddress[] { serverEndpoint.Address }, serverEndpoint.Port);
            }

            // Send requests from clients to server.

            logger.Info($"Executing {TestRequestCount:N0} send/receive requests: clientCount={ClientCount}, concurrentRequests={ConcurrentRequestCount}");

            using (var sw = new LoggerStopWatch(logger))
            {
                var tasks = new List<Task<TcpResponseMessage>>();
                var clientIndices = new List<int>();

                for (int i = 0; i < TestRequestCount; i++)
                {
                    var clientIndex = random.Next() % clients.Length;

                    //if (i % 5000 == 0)
                    if (false && (i % 10000) == (Math.Abs((random.Next()) % 10000)))
                    {
                        clients[clientIndex].Disconnect();

                        clients[clientIndex] = new TcpSocketClient(logger, maxPacketSize: MaxPacketSize, packetQueueCapacity: PacketQueueCapacity);
                        clients[clientIndex].Connect(new IPAddress[] { serverEndpoint.Address }, serverEndpoint.Port);
                    }

                    var client = clients[clientIndex];

                    var sendMessage = new SampleDataMsgPack
                    {
                        ClientId = clientIndex,
                        MyInt = i,
                        TransactionId = 1337,
                        MyString = "abcdefghijklmnopqrstuvwxyz.",
                    };

                    var sendData = MessagePackSerializer.Serialize(sendMessage);

#if PRINT_DATA
                    Logger.Info($"Client {clientIndex} Sending: {sendMessage}");
#endif //PRINT_DATA

                    tasks.Add(client.SendAsync(sendData, 0, (ushort)sendData.Length));
                    clientIndices.Add(clientIndex);

                    if (tasks.Count == ConcurrentRequestCount)
                    {
                        var taskSendAll = Task.WhenAll(tasks);

                        await Task.WhenAny(
                            taskSendAll,
                            Task.Delay(250)).ConfigureAwait(false);

                        if (!taskSendAll.IsCompleted)
                        {
                            logger.Error($"Failed to complete all sends: count={tasks.Count()}, failedCount={tasks.Where(x => !x.IsCompleted).Count()}");

                            tasks.Clear();
                            clientIndices.Clear();

                            continue;
                        }

                        foreach (var task in tasks)
                        {
                            var receivedMessage = MessagePackSerializer.Deserialize<SampleDataMsgPack>(
                                new ReadOnlyMemory<byte>(
                                    task.Result.Data,
                                    task.Result.Offset,
                                    task.Result.Size));
#if PRINT_DATA
                            Logger.Info($"Client {receivedMessage.ClientId} Receive: {receivedMessage}");
#endif //PRINT_DATA
                        }

                        tasks.Clear();
                        clientIndices.Clear();
                    }

#if PRINT_DATA
                        //Logger.Info($"Client {clientIndex} Receive: {receivedObj}");
#endif //PRINT_DATA
                }
            }

            //Logger.Info("Client counts:");

            //for (int i = 0; i < clients.Length; i++)
            //{
            //    Logger.Info($"Client {i}: {(Processor.ClientCounts.ContainsKey(i) ? Processor.ClientCounts[i] : 0)}");
            //}


            for (int i = 0; i < clients.Length; i++)
            {
                clients[i].Disconnect();
            }


            server.Stop();

        }

        [MessagePackObject]
        public class SampleDataMsgPack
        {
            [Key(0)]
            public int ClientId;

            [Key(1)]
            public ushort TransactionId;

            [Key(2)]
            public int MyInt;

            [Key(3)]
            public string MyString;

            public override string ToString()
            {
                return $"ClientId={ClientId}, TransactionId={TransactionId}, MyInt={MyInt}, MyString={MyString}";
            }
        }

#if MOTHBALL
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
#endif
    }
}
