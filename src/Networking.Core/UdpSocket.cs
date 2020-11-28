using Common.Core;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Networking.Core
{
    public abstract class UdpSocketBase<T>
        where T : struct, IPacketSerialization
    {
        protected readonly Socket _socket;

        protected ILogger _logger;
        protected PacketBuffer<T> _packetBuffer;
        protected IPacketEncryptor _encryptor;

        // Save delegate reference to avoid allocation.
        protected static readonly AsyncCallback ReceiveAsyncCallback = new AsyncCallback(ReceiveAsync);
        protected static readonly AsyncCallback SendAsyncCallback = new AsyncCallback(SendAsync);

        private readonly int _closeTimeout;

        protected readonly int MaxPacketSize;

        protected UdpSocketBase(
            ILogger logger, 
            PacketBuffer<T> packetBuffer,
            IPacketEncryptor encryptor,
            int maxPacketSize, 
            int closeTimeout = 0)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._packetBuffer = packetBuffer ?? throw new ArgumentNullException(nameof(packetBuffer));
            this._encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
            this.MaxPacketSize = maxPacketSize;
            this._closeTimeout = closeTimeout;

            //this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this._socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            this._socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
        }

        public void Stop()
        {
            this._socket.Close(this._closeTimeout);
        }

        protected class ReceiveState
        {
            public Socket Socket;
            public PacketBuffer<T> PacketBuffer;
            public EndPoint EndPointFrom;
            public byte[] Buffer;
            public IPacketEncryptor Encryptor;
            public ILogger Logger;
        }

        protected void BeginReceive()
        {
            var state = new ReceiveState
            {
                Socket = this._socket,
                PacketBuffer = this._packetBuffer,
                Encryptor = this._encryptor,
                EndPointFrom = new IPEndPoint(IPAddress.Any, 0),
                Buffer = new byte[MaxPacketSize],
                Logger = this._logger,
            };

            this._socket.BeginReceiveFrom(
                state.Buffer,
                0,
                MaxPacketSize,
                SocketFlags.None,
                ref state.EndPointFrom,
                ReceiveAsyncCallback,
                state);
        }

        private static void ReceiveAsync(IAsyncResult ar)
        {
            ReceiveState state = (ReceiveState)ar.AsyncState;
            int bytesReceived = state.Socket.EndReceiveFrom(ar, ref state.EndPointFrom);

            ushort sequence = PacketEnvelope<T>.GetPacketSequence(state.Buffer, 0, bytesReceived);

            ref var packet = ref state.PacketBuffer.GetPacket(sequence);

            using (var stream = new MemoryStream(state.Buffer, 0, bytesReceived))
            {
                packet.Deserialize(stream, state.Encryptor);
            }

            state.PacketBuffer.SetEndPoint(sequence, (IPEndPoint)state.EndPointFrom);

            // Chain next receive.
            state.Socket.BeginReceiveFrom(
                state.Buffer,
                0,
                state.Buffer.Length,
                SocketFlags.None,
                ref state.EndPointFrom,
                ReceiveAsyncCallback,
                state);
        }

        private static void SendAsync(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            int bytesSent = socket.EndSend(ar);
        }
    }

    public class ClientUdpSocket<T> : UdpSocketBase<T>
        where T : struct, IPacketSerialization
    {
        public ClientUdpSocket(
            ILogger logger, 
            PacketBuffer<T> packetBuffer, 
            IPacketEncryptor encryptor,
            int maxPacketSize) 
            : base(logger, packetBuffer, encryptor, maxPacketSize)
        {
        }

        public EndPoint LocalEndPoint => this._socket.LocalEndPoint;
        public EndPoint RemoteEndPoint => this._socket.RemoteEndPoint;

        public void Start(IPEndPoint ipEndPoint)
        {
            this._socket.Connect(ipEndPoint);
            BeginReceive();
        }

        public SocketError Send(PacketEnvelope<T> packet)
        {
            var data = new byte[MaxPacketSize];
            int size;

            using (var stream = new MemoryStream(data))
            {
                size = packet.Serialize(stream, this._encryptor);
            }

            this._socket.BeginSend(
                data,
                0,
                size,
                SocketFlags.None,
                out SocketError errorCode,
                SendAsyncCallback,
                this._socket);

            return errorCode;
        }
    }

    public class ServerUdpSocket<T> : UdpSocketBase<T>
        where T : struct, IPacketSerialization
    {
        public ServerUdpSocket(
            ILogger logger, 
            PacketBuffer<T> packetBuffer,
            IPacketEncryptor encryptor, 
            int maxPacketSize)
            : base(logger, packetBuffer, encryptor, maxPacketSize)
        {
        }

        public void Start(IPEndPoint ipEndPoint)
        {
            this._socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            this._socket.Bind(ipEndPoint);

            BeginReceive();
        }

        public void SendTo(PacketEnvelope<T> packet, IPEndPoint ipEndPoint)
        {
            var data = new byte[MaxPacketSize];
            int size;

            using (var stream = new MemoryStream(data))
            {
                size = packet.Serialize(stream, this._encryptor);
            }

            this._socket.BeginSendTo(
                data,
                0,
                size,
                SocketFlags.None,
                ipEndPoint,
                SendAsyncCallback,
                this._socket);
        }
    }
}
