namespace Game.Networking
{
    public sealed class NetworkTransportConfig
    {
        public int MaxPacketSize;
        public IPacketEncryptor PacketEncryptor;

        public static readonly NetworkTransportConfig Default = new NetworkTransportConfig
        {
            MaxPacketSize = 512,
            PacketEncryptor = new XorPacketEncryptor(),
        };
    }
}
