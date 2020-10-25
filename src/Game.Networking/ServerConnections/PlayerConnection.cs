using Game.Networking;
using System.Net;

namespace Game.Networking
{
    public struct PlayerConnection
    {
        public WorldId WorldId;
        public PlayerId PlayerId;
        public byte[] PacketEncryptionKey;

        public IPEndPoint EndPoint;
    }
}
