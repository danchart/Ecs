using Game.Networking;
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
}
