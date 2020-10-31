using Common.Core;
using System.Net.Sockets;

namespace Networking.Core.Tcp
{
    public class TcpSocketClient
    {
        private TcpClient _client;
        private NetworkStream _stream;

        public TcpSocketClient(ILogger logger)
        {
            _client = new TcpClient();
        }

        public void Connect(string server, int port)
        {
            _client.Connect(server, port);
            _stream = _client.GetStream();
        }

        public void Disconnect()
        {
            _stream.Close();
            _client.Close();
        }

        public void Send(
            byte[] data,
            int offset,
            int size)
        {
            _stream.Write(data, offset, size);
        }

        public void Read(
            byte[] receiveData,
            int receiveOffset,
            int receiveSize,
            out int receivedBytes)
        {
            receivedBytes = _stream.Read(receiveData, receiveOffset, receiveSize);
        }
    }
}
