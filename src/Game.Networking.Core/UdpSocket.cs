using System;
using System.Net;
using System.Net.Sockets;

namespace Game.Networking.Core
{
    public abstract class UdpSocketBase
    {
        protected State state;

        protected UdpSocketBase(ReceiveBuffer receiveBuffer)
        {
            this.state = new State
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp),
                ReceiveBuffer = receiveBuffer,
                EndPointFrom = new IPEndPoint(IPAddress.Any, 0),
            };
        }

        protected class State
        {
            public Socket Socket;
            public ReceiveBuffer ReceiveBuffer;
            public EndPoint EndPointFrom;
        }

        public void Server(string address, int port)
        {
            Server(new IPEndPoint(IPAddress.Parse(address), port));
        }

        public void Server(IPEndPoint ipEndPoint)
        {
            this.state.Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            this.state.Socket.Bind(ipEndPoint);
            Receive();
        }

        protected void Receive()
        {
            byte[] data;
            int offset, size;
            state.ReceiveBuffer.GetWriteBufferData(out data, out offset, out size);

            this.state.Socket.BeginReceiveFrom(
                data,
                offset,
                size,
                SocketFlags.None,
                ref state.EndPointFrom,
                ReceiveAsyncCallback,
                state);
        }

        private static void ReceiveAsyncCallback(IAsyncResult ar)
        {
            State state = (State)ar.AsyncState;
            int bytesReceived = state.Socket.EndReceiveFrom(ar, ref state.EndPointFrom);

            state.ReceiveBuffer.NextWrite(bytesReceived, (IPEndPoint)state.EndPointFrom);

            // Chain next receive.
            byte[] data;
            int offset, size;
            state.ReceiveBuffer.GetWriteBufferData(out data, out offset, out size);

            state.Socket.BeginReceiveFrom(
                data,
                offset,
                size,
                SocketFlags.None,
                ref state.EndPointFrom,
                ReceiveAsyncCallback,
                state);

            //Console.WriteLine("RECV: {0}: {1}, {2}", epFrom.ToString(), bytes, Encoding.ASCII.GetString(state.buffer, 0, bytes));
        }
    }

    public class UdpSocketClient : UdpSocketBase
    {
        public UdpSocketClient(ReceiveBuffer receiveBuffer) : base(receiveBuffer)
        {
        }

        public void Start(IPEndPoint ipEndPoint)
        {
            this.state.Socket.Connect(ipEndPoint);
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

        public void SendTo(byte[] data)
        {
            this.state.Socket.BeginSendTo(
                data,
                0,
                data.Length,
                SocketFlags.None,
                new IPEndPoint(IPAddress.Any, 0),
                SendAsyncCallback,
                state);
        }

        protected static void SendAsyncCallback(IAsyncResult ar)
        {
            State state = (State)ar.AsyncState;
            int bytesSent = state.Socket.EndSend(ar);
        }
    }

    public class UdpSocketServer : UdpSocketBase
    {
        public UdpSocketServer(ReceiveBuffer receiveBuffer) : base(receiveBuffer)
        {
        }

        public void Start(IPEndPoint ipEndPoint)
        {
            this.state.Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            this.state.Socket.Bind(ipEndPoint);
            Receive();
        }

        public void SendTo(byte[] data, IPEndPoint ipEndPoint)
        {
            this.state.Socket.SendTo(
                data,
                ipEndPoint);
        }
    }

#if MOTHBALL
    public class UdpSocket
    {
        private State state;

        public UdpSocket(ReceiveBuffer receiveBuffer)
        {
            this.state = new State
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp),
                ReceiveBuffer = receiveBuffer,
                EndPointFrom = new IPEndPoint(IPAddress.Any, 0),
            };
        }

        public class State
        {
            public Socket Socket;
            public ReceiveBuffer ReceiveBuffer;
            public EndPoint EndPointFrom;
        }

        public void Server(string address, int port) 
        {
            Server(new IpEndPointProperties(address, port));
        }

        public void Server(IpEndPointProperties ipEndPoint)
        {
            this.state.Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            this.state.Socket.Bind(new IPEndPoint(ipEndPoint.Address, ipEndPoint.Port));
            Receive();
        }


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

        public void SendTo(byte[] data)
        {
            this.state.Socket.BeginSendTo(
                data,
                0,
                data.Length,
                SocketFlags.None,
                new IPEndPoint(IPAddress.Any, 0),
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
                ref state.EndPointFrom, 
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
            int bytesReceived = state.Socket.EndReceiveFrom(ar, ref state.EndPointFrom);

            var ipEndPointFrom = (IPEndPoint)state.EndPointFrom;

            state.ReceiveBuffer.NextWrite(bytesReceived, ipEndPointFrom.Address.Address, ipEndPointFrom.Port);

            // Chain next receive.
            byte[] data;
            int offset, size;
            state.ReceiveBuffer.GetWriteBufferData(out data, out offset, out size);

            state.Socket.BeginReceiveFrom(
                data,
                offset,
                size,
                SocketFlags.None,
                ref state.EndPointFrom,
                ReceiveAsyncCallback,
                state);

            //Console.WriteLine("RECV: {0}: {1}, {2}", epFrom.ToString(), bytes, Encoding.ASCII.GetString(state.buffer, 0, bytes));
        }
    }
#endif
}
