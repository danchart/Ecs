using System;
using System.Net;
using System.Net.Sockets;

namespace Game.Networking
{
    public class UdpSocket
    {
        private State state;

        public UdpSocket(ReceiveBuffer receiveBuffer)
        {
            this.state = new State
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp),
                EndpointFrom = new IPEndPoint(IPAddress.Any, 0),
                ReceiveBuffer = receiveBuffer,
            };
        }

        public class State
        {
            public Socket Socket;
            public EndPoint EndpointFrom;
            public ReceiveBuffer ReceiveBuffer;
        }

        public struct ReceiveBuffer
        {
            public readonly int BufferSize;
            private readonly byte[] _data;

            private int _bytedReceived;

            public ReceiveBuffer(int bufferSize)
            {
                this.BufferSize = bufferSize;
                this._data = new byte[bufferSize];
                this._bytedReceived = 0;
            }

            public byte[] GetBuffer() => this._data;
            public void Commit(int bytesReceived) => this._bytedReceived = bytesReceived;
        }

        public void Server(string address, int port)
        {
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
            Receive();
        }

        public void Client(string address, int port)
        {
            _socket.Connect(IPAddress.Parse(address), port);
            Receive();
        }

        public void Send(byte[] data)
        {
            this.state.Socket.BeginSend(
                data, 
                0, 
                data.Length, 
                SocketFlags.None,
                SendAsyncCallback,
                state);
        }

        private void Receive(ReceiveBuffer buffer)
        {
            this.state.Socket.BeginReceiveFrom(
                buffer.GetBuffer(), 
                0,
                buffer.BufferSize, 
                SocketFlags.None, 
                ref this.state.EndpointFrom, 
                ReceiveAsyncCallback,
                state);
        }

        private static void SendAsyncCallback(IAsyncResult ar)
        {
            State state = (State)ar.AsyncState;
            int bytesSent = state.Socket.EndSend(ar);
        }

        private static void ReceiveAsyncCallback(IAsyncResult ar)
        {
            State state = (State)ar.AsyncState;
            int bytesReceived = state.Socket.EndReceiveFrom(ar, ref state.EndpointFrom);

            state.ReceiveBuffer.Commit(bytesReceived);
            //Console.WriteLine("RECV: {0}: {1}, {2}", epFrom.ToString(), bytes, Encoding.ASCII.GetString(state.buffer, 0, bytes));
        }
    }
}
