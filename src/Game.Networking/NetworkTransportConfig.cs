using Networking.Core;

namespace Game.Networking
{
    public sealed class NetworkTransportConfig
    {
        public int MaxPacketSize;
        public IPacketEncryptor PacketEncryptor;

        // TODO: Capacity should be larger for server, smaller for client transport layers.
        public int ReceivePacketQueueCapacity = 256;
        public int SendPacketQueueCapacity = 128;

        public static readonly NetworkTransportConfig Default = new NetworkTransportConfig
        {
            MaxPacketSize = 512,
            PacketEncryptor = new XorPacketEncryptor(),
        };
    }
}
