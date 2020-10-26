using Common.Core;
using Game.Networking;
using Game.Networking.Core;
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

        public static readonly UdpPacketTransportConfig Default = new UdpPacketTransportConfig();
    }

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

        public void SendPacket(IPEndPoint endPoint, in ServerPacket packet)
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

        //public async Task<Packet> ReceiveAsync()
        //{ 
        //    var receiveResult = await this.Client.ReceiveAsync();

        //    var stream = new MemoryStream(receiveResult.Buffer, 0, receiveResult.Buffer.Length);

        //    Packet packet = new Packet();

        //    packet.Deserialize(stream);

        //    return packet;
        //}
    }
}
