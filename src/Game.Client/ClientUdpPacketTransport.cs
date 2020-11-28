using Common.Core;
using Game.Networking;
using Networking.Core;
using System;
using System.IO;
using System.Net;

namespace Game.Client
{
    public class ClientUdpPacketTransport
    {
        private readonly ClientUdpSocket _socket;
        private readonly ReceiveBuffer _receiveBuffer;

        private readonly IPEndPoint _remoteEndPoint;
        private readonly IPacketEncryptor _packetEncryption;

        private ByteArrayPool _packetSerializationBytePool;

        public ClientUdpPacketTransport(
            ILogger logger,
            IPacketEncryptor packetEncryption,
            NetworkTransportConfig transportConfig,
            IPEndPoint remoteEndPoint)
        {
            this._packetEncryption = packetEncryption ?? throw new ArgumentNullException(nameof(packetEncryption));
            this._remoteEndPoint = remoteEndPoint;

            this._receiveBuffer = new ReceiveBuffer(transportConfig.MaxPacketSize, transportConfig.ReceivePacketQueueCapacity);
            this._socket = new ClientUdpSocket(logger, this._receiveBuffer);

            this._packetSerializationBytePool = new ByteArrayPool(transportConfig.MaxPacketSize, transportConfig.SendPacketQueueCapacity);
        }

        public ReceiveBuffer ReceiveBuffer => this._receiveBuffer;

        public void Start()
        {
            this._socket.Start(this._remoteEndPoint);
        }

        public FrameNumber SendPacket(in ClientPacket packet)
        {
            var bufferPoolIndex = this._packetSerializationBytePool.New();
            var data = this._packetSerializationBytePool.GetBuffer(bufferPoolIndex);

            int size;
            using (var stream = new MemoryStream(data))
            {
                size = packet.Serialize(stream, this._packetEncryption);
            }

            this._socket.Send(data);

            this._packetSerializationBytePool.Free(bufferPoolIndex);
        }
    }
}
