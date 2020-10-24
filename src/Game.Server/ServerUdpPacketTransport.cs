using Common.Core;
using Game.Networking;
using Game.Networking.Core;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Game.Server
{
    public class UdpPacketTransportConfig
    {
        public int MaxPacketSize = 768;
        public int PacketReceiveQueueCapacity = 256;
        public int PacketSendQueueCapacity = 128;

        public IPEndPoint HostIpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000);

        public IPacketEncryption PacketEncryption = new XorPacketEncryption();
    }

    public class ServerUdpPacketTransport
    {
        private UdpSocketServer UdpSocket;
        private ReceiveBuffer ReceiveBuffer;

        private readonly UdpPacketTransportConfig _config;

        private Dictionary<PlayerId, TransportClient> PlayerIdToIpEndPoint;

        private ByteArrayPool _packetSerializationBytePool;

        public ServerUdpPacketTransport(ILogger logger, UdpPacketTransportConfig config)
        {
            this._config = config;

            this.ReceiveBuffer = new ReceiveBuffer(config.MaxPacketSize, config.PacketReceiveQueueCapacity);
            this.UdpSocket = new UdpSocketServer(logger, this.ReceiveBuffer);
            this.PlayerIdToIpEndPoint = new Dictionary<PlayerId, TransportClient>();

            this._packetSerializationBytePool = new ByteArrayPool(config.MaxPacketSize, config.PacketSendQueueCapacity);
        }

        public void Start()
        {
            this.UdpSocket.Start(this._config.HostIpEndPoint);
        }

        public void SendPacket(PlayerId playerId, in ServerPacket packet)
        {
            var client = this.PlayerIdToIpEndPoint[playerId];

            var bufferPoolIndex = this._packetSerializationBytePool.New();
            var data = this._packetSerializationBytePool.GetBuffer(bufferPoolIndex);

            using (var stream = new MemoryStream(data))
            {
                packet.Serialize(stream, measureOnly: false, packetEncryption: this._config.PacketEncryption);
            }

            this.UdpSocket.SendTo(data, client.EndPoint);

            this._packetSerializationBytePool.Free(bufferPoolIndex);
        }

        //public async Task<Packet> ReceiveAsync()
        //{ 
        //    var receiveResult = await this.Client.ReceiveAsync();

        //    var stream = new MemoryStream(receiveResult.Buffer, 0, receiveResult.Buffer.Length);

        //    Packet packet = new Packet();

        //    packet.Deserialize(stream);

        //    return packet;
        //}

        private class TransportClient
        {
            public IPEndPoint EndPoint;
            public byte[] Key;
        }
    }
}
