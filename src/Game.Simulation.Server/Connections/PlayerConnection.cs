using Ecs.Core;
using Game.Networking;

namespace Game.Simulation.Server
{
    public struct PlayerConnection
    {
        public PlayerId PlayerId;

        public Entity Entity;

        public PlayerReplicationData ReplicationData;

        public byte[] PacketEncryptionKey; 
    }
}
