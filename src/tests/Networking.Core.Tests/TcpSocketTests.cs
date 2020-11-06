using Common.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            Serializer.Deserialize(data, 0, data.Length, out string text);

            var responseData = new byte[1024];

            Serializer.Serialize($"Received string: {text}", responseData, out int count);

            var responseDataSlice = new byte[count];

            Array.Copy(responseData, responseDataSlice, count);

            return responseDataSlice;
        }

        [Fact]
        private async void TestAsync()
        {
            // Testing constants:

            const int MaxPacketSize = 256;
            const int PacketQueueCapacity = 256;

            const int TestRequestCount = 1000;
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

                    var requestData = new byte[1024];
                    Serializer.Serialize($"Client data {i}", requestData, out int count);

#if PRINT_DATA
                    Logger.Info($"Client {clientIndex} Sending: {sendMessage}");
#endif //PRINT_DATA

                    tasks.Add(client.SendAsync(requestData, 0, (ushort)count));
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
                            var responseData = new byte[1024];

                            Serializer.Deserialize(task.Result.Data, task.Result.Offset, task.Result.Size, out string text);

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

        private class Serializer
        {
            public static void Serialize(string text, byte[] data, out int count)
            {
                count = Encoding.ASCII.GetBytes(text, 0, text.Length, data, 0);
            }

            public static int Deserialize(byte[] data, int offset, int count, out string text)
            {
                text = Encoding.ASCII.GetString(data);
                offset = 0;

                return text.Length;
            }
        }

        //[MessagePackObject]
        //public class SampleDataMsgPack
        //{
        //    [Key(0)]
        //    public int ClientId;

        //    [Key(1)]
        //    public ushort TransactionId;

        //    [Key(2)]
        //    public int MyInt;

        //    [Key(3)]
        //    public string MyString;

        //    public override string ToString()
        //    {
        //        return $"ClientId={ClientId}, TransactionId={TransactionId}, MyInt={MyInt}, MyString={MyString}";
        //    }
    //}
    }
}
