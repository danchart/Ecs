using Common.Core;
using Networking.Core;
using System;
using System.Collections.Generic;

namespace Database.Server
{
    public class DatabaseServerChannelIncoming
    {
        private readonly TcpSocketListener _tcpListener;
        private readonly List<TcpReceiveBuffer> _buffers;

        private readonly ILogger _logger;

        private object _lock = new object();

        private bool _isRunning;

        public DatabaseServerChannelIncoming(TcpSocketListener tcpListener, ILogger logger)
        {
            this._tcpListener = tcpListener ?? throw new ArgumentNullException(nameof(tcpListener));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this._tcpListener.Clients.OnClientAdded += Clients_OnClientAdded;
            this._tcpListener.Clients.OnClientRemoved += Clients_OnClientRemoved;

            this._isRunning = false;
        }

        public bool IsRunning => this._isRunning;

        public void Stop()
        {
            this._isRunning = false;
        }

        public void Run()
        {
            this._isRunning = true;

            //var serializer = new Serializer();

            //var sendStr = $"Server Message: {new string('s', 128)}";

            while (this._isRunning)
            {
                lock (_lock)
                {
                    foreach (var buffer in _buffers)
                    {
                        if (buffer.GetReadData(out byte[] data, out int offset, out int count))
                        {
                            serializer.Deserialize(data, 0, count, out string recvText);
#if PRINT_DATA
                        _logger.Info($"Server received: {recvText}");
#endif //PRINT_DATA

                            buffer.GetClient(out _remoteClient);

                            buffer.NextRead(closeConnection: false);

                            var stream = _remoteClient.GetStream();

                            var writeData = new byte[256];

                            serializer.Serialize(sendStr, writeData, out int writeCount);

                            stream.Write(writeData, 0, writeCount);
                        }
                    }
                }
            }
        }


        private void Clients_OnClientAdded(object sender, TcpSocketListener.TcpClientsEventArgs e)
        {
            lock (_lock)
            {
                _buffers.Add(e.ReceiveBuffer);
            }
        }

        private void Clients_OnClientRemoved(object sender, TcpSocketListener.TcpClientsEventArgs e)
        {
            lock (_lock)
            {
                _buffers.Remove(e.ReceiveBuffer);
            }
        }

    }
}
