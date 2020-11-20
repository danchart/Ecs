using Common.Core;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Networking.Core
{
    public abstract class UdpSocketBase
    {
        protected readonly State _state;

        protected UdpSocketBase(ILogger logger, ReceiveBuffer receiveBuffer)
        {
            this._state = new State
            {
                //Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp),
                Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp),
                ReceiveBuffer = receiveBuffer ?? throw new ArgumentNullException(nameof(receiveBuffer)),
                EndPointFrom = new IPEndPoint(IPAddress.Any, 0),
                Logger = logger ?? throw new ArgumentNullException(nameof(logger)),
                ReceiveAsync = ReceiveAsync,
                SendAsync = SendAsync,
            };

            this._state.Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
        }

        protected class State
        {
            public Socket Socket;
            public ReceiveBuffer ReceiveBuffer;
            public EndPoint EndPointFrom;
            public ILogger Logger;

            // Save delegate reference to avoid allocation.
            public AsyncCallback ReceiveAsync; 
            public AsyncCallback SendAsync;
        }

        public void Server(string address, int port)
        {
            Server(new IPEndPoint(IPAddress.Parse(address), port));
        }

        public void Server(IPEndPoint ipEndPoint)
        {
            this._state.Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            this._state.Socket.Bind(ipEndPoint);
            BeginReceive();
        }

        protected void BeginReceive()
        {
            byte[] data;
            int offset, size;
            this._state.ReceiveBuffer.GetWriteData(out data, out offset, out size);

            this._state.Socket.BeginReceiveFrom(
                data,
                offset,
                size,
                SocketFlags.None,
                ref this._state.EndPointFrom,
                this._state.ReceiveAsync,
                this._state);
        }

        private static void ReceiveAsync(IAsyncResult ar)
        {
            State state = (State)ar.AsyncState;
            int bytesReceived = state.Socket.EndReceiveFrom(ar, ref state.EndPointFrom);

            state.ReceiveBuffer.NextWrite(bytesReceived, (IPEndPoint)state.EndPointFrom);

            byte[] data;
            int offset, size;
            bool isWriteBufferWait = false;
            while (!state.ReceiveBuffer.GetWriteData(out data, out offset, out size))
            {
                if (!isWriteBufferWait)
                {
                    isWriteBufferWait = true;

                    state.Logger.Error("UDP server socket is out of writable buffer space.");
                }
                // Wait for write queue to become available.
                Thread.Sleep(6);
            }

            // Chain next receive.
            state.Socket.BeginReceiveFrom(
                data,
                offset,
                size,
                SocketFlags.None,
                ref state.EndPointFrom,
                state.ReceiveAsync,
                state);
        }

        private static void SendAsync(IAsyncResult ar)
        {
            State state = (State)ar.AsyncState;
            int bytesSent = state.Socket.EndSend(ar);
        }
    }

    public class ClientUdpSocket : UdpSocketBase
    {
        private static readonly IPEndPoint IpAddressAny = new IPEndPoint(IPAddress.Any, 0);

        public ClientUdpSocket(ILogger logger, ReceiveBuffer receiveBuffer) 
            : base(logger, receiveBuffer)
        {
        }

        public void Start(IPEndPoint ipEndPoint)
        {
            this._state.Socket.Connect(ipEndPoint);
            BeginReceive();
        }

        public SocketError Send(byte[] data)
        {
            this._state.Socket.BeginSend(
                data,
                0,
                data.Length,
                SocketFlags.None,
                out SocketError errorCode,
                this._state.SendAsync,
                _state);

            return errorCode;
        }
    }

    public class ServerUdpSocket : UdpSocketBase
    {
        public ServerUdpSocket(ILogger logger, ReceiveBuffer receiveBuffer) 
            : base(logger, receiveBuffer)
        {
        }

        public void Start(IPEndPoint ipEndPoint)
        {
            this._state.Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            this._state.Socket.Bind(ipEndPoint);

            BeginReceive();
        }

        public void SendTo(byte[] data, IPEndPoint ipEndPoint)
        {
            SendTo(data, 0, data.Length, ipEndPoint);
        }

        public void SendTo(byte[] data, int offset, int length, IPEndPoint ipEndPoint)
        {
            this._state.Socket.BeginSendTo(
                data,
                offset,
                length,
                SocketFlags.None,
                ipEndPoint,
                this._state.SendAsync,
                this._state);
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
