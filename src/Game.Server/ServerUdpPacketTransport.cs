using Common.Core;
using Game.Networking;
using Networking.Core;
using System;
using System.IO;
using System.Net;

namespace Game.Server
{
    public class ServerUdpPacketTransport
    {
        private readonly ServerUdpSocket _udpSocket;
        private readonly ReceiveBuffer _receiveBuffer;

        private readonly IPEndPoint _endPoint;
        private readonly IPacketEncryptor _packetEncryption;

        private ByteArrayPool _packetSerializationBytePool;

        public ServerUdpPacketTransport(
            ILogger logger, 
            IPacketEncryptor packetEncryption, 
            NetworkTransportConfig transportConfig,
            UdpServerConfig udpServerConfig)
        {
            this._packetEncryption = packetEncryption ?? throw new ArgumentNullException(nameof(packetEncryption));
            this._endPoint = udpServerConfig.HostIpEndPoint;

            this._receiveBuffer = new ReceiveBuffer(transportConfig.MaxPacketSize, transportConfig.ReceivePacketQueueCapacity);
            this._udpSocket = new ServerUdpSocket(logger, this._receiveBuffer);

            this._packetSerializationBytePool = new ByteArrayPool(transportConfig.MaxPacketSize, transportConfig.SendPacketQueueCapacity);
        }

        public ReceiveBuffer ReceiveBuffer => this._receiveBuffer;

        public void Start()
        {
            this._udpSocket.Start(this._endPoint);
        }

        public void Stop()
        {
            this._udpSocket.Stop();
        }

        public void SendPacket(IPEndPoint endPoint, in ServerPacketEnvelope packet)
        {
            var bufferPoolIndex = this._packetSerializationBytePool.New();
            var data = this._packetSerializationBytePool.GetBuffer(bufferPoolIndex);

            int size;
            using (var stream = new MemoryStream(data))
            {
                size = packet.Serialize(stream, measureOnly: false, packetEncryption: this._packetEncryption);
            }

            this._udpSocket.SendTo(data, endPoint);

            this._packetSerializationBytePool.Free(bufferPoolIndex);
        }
    }
}
