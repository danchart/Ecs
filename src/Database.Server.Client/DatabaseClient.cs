using Common.Core;
using Database.Server.Protocol;
using Networking.Server;
using System.Net;

namespace Database.Server.Client
{
    public class DatabaseClient
    {
        private TcpSocketClient _client;

        public DatabaseClient(ILogger logger, int messageQueueCapacity)
        {
            this._client = new TcpSocketClient(logger, ProtocolConstants.MaxTcpMessageSize, messageQueueCapacity);
        }

        public void Connect(IPEndPoint ipEndPoint)
        {
            this._client.Connect(ipEndPoint);
        }

        public void Disconnect()
        {
            this._client.Disconnect();
        }
    }
}
