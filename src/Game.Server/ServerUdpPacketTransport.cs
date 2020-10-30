using Common.Core;
using Game.Networking;
using Networking.Core;
using System.IO;
using System.Net;

namespace Game.Server
{
    public class ServerUdpPacketTransport
    {
        private readonly ServerUdpSocket _udpSocket;
        private readonly ReceiveBuffer _receiveBuffer;

        private readonly UdpPacketTransportConfig _config;

        private ByteArrayPool _packetSerializationBytePool;

        public ServerUdpPacketTransport(ILogger logger, UdpPacketTransportConfig config)
        {
            this._config = config;

            this._receiveBuffer = new ReceiveBuffer(config.MaxPacketSize, config.PacketReceiveQueueCapacity);
            this._udpSocket = new ServerUdpSocket(logger, this._receiveBuffer);

            this._packetSerializationBytePool = new ByteArrayPool(config.MaxPacketSize, config.PacketSendQueueCapacity);
        }

        public ReceiveBuffer ReceiveBuffer => this._receiveBuffer;

        public void Start()
        {
            this._udpSocket.Start(this._config.HostIpEndPoint);
        }

        public void SendPacket(IPEndPoint endPoint, in ServerPacketEnvelope packet)
        {
            var bufferPoolIndex = this._packetSerializationBytePool.New();
            var data = this._packetSerializationBytePool.GetBuffer(bufferPoolIndex);

            int size;
            using (var stream = new MemoryStream(data))
            {
                size = packet.Serialize(stream, measureOnly: false, packetEncryption: this._config.PacketEncryption);
            }

            this._udpSocket.SendTo(data, endPoint);

            this._packetSerializationBytePool.Free(bufferPoolIndex);
        }
    }
}
