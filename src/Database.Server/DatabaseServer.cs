using Common.Core;
using Networking.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Database.Server
{
    public sealed class DatabaseServer
    {
        private readonly TcpServer _tcpServer;

        private readonly ServerConfig _config;

        public DatabaseServer(ILogger logger, ServerConfig config)
        {
            this._tcpServer = new TcpServer(
                logger, 
                config.TcpClientCapacity, 
                config.MaxTcpMessageSize, 
                config.TcpMessageQueueCapacity);

            this._config = config;
        }

        public bool IsRunning => this._tcpServer.IsRunning;

        public void Start()
        {
            this._tcpServer.Start(
                _config.HostIpEndPoint,
                ProcessAsync);
        }

        public void Stop()
        {
            this._tcpServer.Stop();
        }

        private static async Task<byte[]> ProcessAsync(byte[] data, CancellationToken token)
        {
            Serializer.Deserialize(data, 0, data.Length, out string text);

            var responseData = new byte[1024];

            Serializer.Serialize($"Received string: {text}", responseData, out int count);

            var responseDataSlice = new byte[count];

            Array.Copy(responseData, responseDataSlice, count);

            return responseDataSlice;
        }
    }
}
