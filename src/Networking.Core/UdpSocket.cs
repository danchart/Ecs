using Common.Core;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Networking.Core
{
    public delegate void OnPacketAckedDelegate(ushort sequence);

    public abstract class UdpSocketBase<T>
        where T : struct, IPacketSerialization
    {
        public OnPacketAckedDelegate OnAcked;

        protected int _sequence; // not ushort so we can use Interlocked.Increment(), must convert to ushort.ss

        protected readonly Socket _socket;

        protected readonly ILogger _logger;
        protected readonly PacketSequenceBuffer _localSequenceBuffer;
        protected readonly PacketSequenceBuffer _remoteSequenceBuffer;
        protected readonly PacketBuffer<T> _packetBuffer;
        protected readonly IPacketEncryptor _encryptor;

        // Save delegate reference to avoid allocation.
        protected static readonly AsyncCallback ReceiveAsyncCallback = new AsyncCallback(ReceiveAsync);
        protected static readonly AsyncCallback SendAsyncCallback = new AsyncCallback(SendAsync);

        private readonly int _closeTimeout;

        protected readonly int MaxPacketSize;

        protected UdpSocketBase(
            ILogger logger,
            PacketSequenceBuffer localSequenceBuffer,
            PacketSequenceBuffer remoteSequenceBuffer,
            PacketBuffer<T> packetBuffer,
            IPacketEncryptor encryptor,
            int maxPacketSize, 
            int closeTimeout = 0)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._localSequenceBuffer = localSequenceBuffer ?? throw new ArgumentNullException(nameof(localSequenceBuffer));
            this._remoteSequenceBuffer = remoteSequenceBuffer ?? throw new ArgumentNullException(nameof(remoteSequenceBuffer));
            this._packetBuffer = packetBuffer ?? throw new ArgumentNullException(nameof(packetBuffer));
            this._encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
            this.MaxPacketSize = maxPacketSize;

            this._sequence = 0;
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
            public PacketSequenceBuffer LocalSequenceBuffer;
            public PacketSequenceBuffer RemoteSequenceBuffer;
            public PacketBuffer<T> PacketBuffer;
            public EndPoint EndPointFrom;
            public byte[] Data;
            public IPacketEncryptor Encryptor;
            public OnPacketAckedDelegate OnAcked;
            public ILogger Logger;
        }

        protected void BeginReceive()
        {
            var state = new ReceiveState
            {
                Socket = this._socket,
                LocalSequenceBuffer = this._localSequenceBuffer,
                RemoteSequenceBuffer = this._remoteSequenceBuffer,
                PacketBuffer = this._packetBuffer,
                Encryptor = this._encryptor,
                EndPointFrom = new IPEndPoint(IPAddress.Any, 0),
                Data = new byte[MaxPacketSize],
                OnAcked = this.OnAcked,
                Logger = this._logger,
            };

            this._socket.BeginReceiveFrom(
                state.Data,
                0,
                MaxPacketSize,
                SocketFlags.None,
                ref state.EndPointFrom,
                ReceiveAsyncCallback,
                state);
        }

        protected ushort GetNextSequence()
        {
            return (ushort)(Interlocked.Increment(ref this._sequence) % ushort.MaxValue);
        }

        private static void ReceiveAsync(IAsyncResult ar)
        {
            ReceiveState state = (ReceiveState)ar.AsyncState;
            int bytesReceived = state.Socket.EndReceiveFrom(ar, ref state.EndPointFrom);

            ushort sequence = PacketEnvelope<T>.GetPacketSequence(state.Data, 0, bytesReceived);
            PacketEnvelopeHeader packetHeader = default;

            ref var packet = ref state.PacketBuffer.Add(sequence, (IPEndPoint)state.EndPointFrom);

            using (var stream = new MemoryStream(state.Data, 0, bytesReceived))
            {
                PacketEnvelope<T>.Deserialize(stream, state.Encryptor, ref packetHeader, ref packet);
            }

            state.RemoteSequenceBuffer.Insert(packetHeader.Sequence);

            state.LocalSequenceBuffer.Update(
                packetHeader.Ack, 
                packetHeader.AckBitfield, 
                state.OnAcked);

            // Chain next receive.
            state.Socket.BeginReceiveFrom(
                state.Data,
                0,
                state.Data.Length,
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
            PacketSequenceBuffer localSequenceBuffer,
            PacketSequenceBuffer remoteSequenceBuffer,
            PacketBuffer<T> packetBuffer, 
            IPacketEncryptor encryptor,
            int maxPacketSize) 
            : base(
                  logger, 
                  localSequenceBuffer, 
                  remoteSequenceBuffer,
                  packetBuffer, 
                  encryptor, 
                  maxPacketSize)
        {
        }

        public EndPoint LocalEndPoint => this._socket.LocalEndPoint;
        public EndPoint RemoteEndPoint => this._socket.RemoteEndPoint;

        public void Start(IPEndPoint ipEndPoint)
        {
            this._socket.Connect(ipEndPoint);
            BeginReceive();
        }

        public ushort Send(in T packet)
        {
            var data = new byte[MaxPacketSize];
            int size;

            PacketEnvelope<T> envelope = new PacketEnvelope<T>
            {
                Header = new PacketEnvelopeHeader
                {
                    Sequence = GetNextSequence(),
                    Ack = this._remoteSequenceBuffer.Ack,
                    AckBitfield = this._remoteSequenceBuffer.GetAckBitfield()
                }
            };

            using (var stream = new MemoryStream(data))
            {
                size = envelope.Serialize(
                    stream, 
                    packet,
                    this._encryptor);
            }

            this._socket.BeginSend(
                data,
                0,
                size,
                SocketFlags.None,
                out SocketError errorCode,
                SendAsyncCallback,
                this._socket);

            if (errorCode != SocketError.Success)
            {
                this._logger.VerboseError($"Socket send error: errorCode={errorCode}");
            }

            return envelope.Header.Sequence;
        }
    }

    public class ServerUdpSocket<T> : UdpSocketBase<T>
        where T : struct, IPacketSerialization
    {
        public ServerUdpSocket(
            ILogger logger,
            PacketSequenceBuffer localSequenceBuffer,
            PacketSequenceBuffer remoteSequenceBuffer,
            PacketBuffer<T> packetBuffer,
            IPacketEncryptor encryptor, 
            int maxPacketSize)
            : base(
                  logger, 
                  localSequenceBuffer,
                  remoteSequenceBuffer, 
                  packetBuffer, 
                  encryptor, 
                  maxPacketSize)
        {
        }

        public void Start(IPEndPoint ipEndPoint)
        {
            this._socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            this._socket.Bind(ipEndPoint);

            BeginReceive();
        }

        public ushort SendTo(in T packet, IPEndPoint ipEndPoint)
        {
            var data = new byte[MaxPacketSize];
            int size;

            PacketEnvelope<T> envelope = new PacketEnvelope<T>
            {
                Header = new PacketEnvelopeHeader
                {
                    Sequence = GetNextSequence(),
                    Ack = this._remoteSequenceBuffer.Ack,
                    AckBitfield = this._remoteSequenceBuffer.GetAckBitfield()
                }
            };

            using (var stream = new MemoryStream(data))
            {
                size = envelope.Serialize(
                    stream,
                    packet,
                    this._encryptor);
            }

            this._socket.BeginSendTo(
                data,
                0,
                size,
                SocketFlags.None,
                ipEndPoint,
                SendAsyncCallback,
                this._socket);

            return envelope.Header.Sequence;
        }
    }
}
