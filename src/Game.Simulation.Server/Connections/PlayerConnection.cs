using Game.Networking;

namespace Game.Simulation.Server
{
    public struct PlayerConnection
    {
        public PlayerId PlayerId;
        public byte[] PacketEncryptionKey;
    }
}
