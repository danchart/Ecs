using Common.Core;
using Networking.Core;

namespace Database.Server
{
    public sealed class DatabaseServer
    {
        private readonly TcpSocketListener _tcpListener;

        private readonly TcpReceiveBuffer _receiveBuffer;

        private readonly ServerConfig _config;

        public DatabaseServer(ILogger logger, ServerConfig config)
        {
            this._receiveBuffer = new TcpReceiveBuffer(config.MaxTcpPacketSize, config.TcpPacketReceiveQueueCapacity);
            this._tcpListener = new TcpSocketListener(logger, this._receiveBuffer);
            this._config = config;
        }

        public bool IsRunning => this._tcpListener.IsRunning;

        public void Start()
        {
            this._tcpListener.Start(_config.HostIpEndPoint);
        }

        public void Stop()
        {
            this._tcpListener.Stop();
        }
    }
}
