using Common.Core;
using Database.Server.Protocol;
using Networking.Tcp;
using System;
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
                ProtocolConstants.MaxTcpMessageSize, 
                config.TcpMessageQueueCapacity);

            this._config = config;
        }

        public bool IsRunning => this._tcpServer.IsRunning;

        public void Start()
        {
            this._tcpServer.Start(
                ProtocolHelper.GetServerEndPointFromIpAddress(_config.HostIpAddress),
                ProcessAsync);
        }

        public void Stop()
        {
            this._tcpServer.Stop();
        }

        private static async Task<byte[]> ProcessAsync(byte[] data, CancellationToken token)
        {
            var messageTypeId = (Contracts.DatabaseMessageIds) BitConverter.ToUInt16(data, 0);

            switch (messageTypeId)
            {
                case Contracts.DatabaseMessageIds.GetPlayer:
                    {
                        var request = Contracts.Serializer.Deserialize<Contracts.GetPlayerRequest>(data, 0, data.Length - 2);

                        return Contracts.Serializer.Serialize(new Contracts.GetPlayerResponse
                        {
                            Name = "John Wayne",
                        });
                    }
            }

            // Unknown message id.
            return null;
        }
    }
}
