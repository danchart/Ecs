using Game.Networking;
using System.Net;

namespace Game.Simulation.Server
{
    public struct PlayerConnection
    {
        public WorldId WorldId;
        public PlayerId PlayerId;
        public byte[] PacketEncryptionKey;

        public IPEndPoint EndPoint;
    }
}
