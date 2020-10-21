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

        /// <summary>
        /// Starts UDP server/receiver.
        /// </summary>
        public void Server(string address, int port)
        {
            this.state.Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            this.state.Socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
            Receive();
        }

        /// <summary>
        /// Starts UDP client/sender.
        /// </summary>
        public void Client(string address, int port)
        {
            this.state.Socket.Connect(IPAddress.Parse(address), port);
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

        private void Receive()
        {
            byte[] data;
            int offset, size;
            state.ReceiveBuffer.GetWriteBufferData(out data, out offset, out size);

            this.state.Socket.BeginReceiveFrom(
                data, 
                offset,
                size, 
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

            state.ReceiveBuffer.NextWrite(bytesReceived);

            // Chain next receive.
            byte[] data;
            int offset, size;
            state.ReceiveBuffer.GetWriteBufferData(out data, out offset, out size);

            state.Socket.BeginReceiveFrom(
                data,
                offset,
                size,
                SocketFlags.None,
                ref state.EndpointFrom,
                ReceiveAsyncCallback,
                state);
        }
    }
}
