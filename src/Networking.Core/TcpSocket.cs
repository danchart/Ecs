using Common.Core;
using Networking.Core;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Database.Server
{
    public class TcpSocketListener
    {
        private bool _isRunning;

        private TcpListener _listener;

        private readonly TcpReceiveBuffer _receiveBuffer;

        private readonly ILogger _logger;

        public TcpSocketListener(ILogger logger, TcpReceiveBuffer receiveBuffer)
        {
            this._isRunning = false;

            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this._receiveBuffer = receiveBuffer;
        }

        public bool IsRunning => this._isRunning;

        public void Start(IPEndPoint ipEndPoint)
        {
            // From: https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener?view=netcore-3.1

            this._listener = new TcpListener(ipEndPoint);
            this._isRunning = true;

            // Start listening for client requests.
            _listener.Start();

            _listener.BeginAcceptTcpClient(ProcessRequest, _listener);

            this._logger.Info("Waiting for a connection... ");
        }

        private void ProcessRequest(IAsyncResult ar)
        {
            if (!_isRunning)
            {
                return;
            }

            var listener = ar.AsyncState as TcpListener;

            if (listener== null)
            {
                return;
            }

            // Begin waiting for the next request.
            listener.BeginAcceptTcpClient(ProcessRequest, listener);

            using (TcpClient client = listener.EndAcceptTcpClient(ar))
            {
                this._logger.Info($"Connected. RemoteEp={client.Client.RemoteEndPoint}");

                this._receiveBuffer.GetWriteData(out byte[] data, out int offset, out int size);

                // Get a stream object for reading and writing
                using (NetworkStream stream = client.GetStream())
                {
                    int totalReadCount = 0;
                    int readCount;

                    bool isReadSuccess = true;

                    // Loop to receive all the data sent by the client.
                    while ((readCount = stream.Read(data, offset + totalReadCount, size - totalReadCount)) != 0)
                    {
                        totalReadCount += readCount;

                        if (totalReadCount > size)
                        {
                            isReadSuccess = false;

                            this._logger.Error($"Received TCP packet exceeded buffer size: bufferSize={size}");

                            break;
                        }
                    }

                    if (isReadSuccess)
                    {
                        this._receiveBuffer.NextWrite(totalReadCount, client);
                    }
                }

                // Shutdown and end connection
                client.Close();
            }
        }

        public void Stop()
        {
            this._isRunning = false;

            // Wait a short period to ensure the cancellation flag has propogated. 
            Thread.Sleep(100);

            _listener.Stop();
        }
    }

    public class TcpSocketClient
    {
        private TcpClient _client;

        public TcpSocketClient(ILogger logger)
        {
            _client = new TcpClient();
        }

        public void Connect(string server, int port)
        {
            _client.Connect(server, port);
        }

        public void Disconnect()
        {
            _client.Close();
        }

        public void Send(
            byte[] data, 
            int offset, 
            int size,
            byte[] receiveData,
            int receiveOffset,
            int receiveSize,
            out int receivedBytes)
        {
            using (NetworkStream stream = _client.GetStream())
            {
                stream.Write(data, offset, size);

                receivedBytes = stream.Read(receiveData, receiveOffset, receiveSize);

                stream.Close();
            }
        }
    }
}
