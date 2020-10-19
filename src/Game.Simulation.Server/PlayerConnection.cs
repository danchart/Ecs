using Ecs.Core;

namespace Game.Simulation.Server
{
    public struct PlayerConnection
    {
        public int PlayerId;

        public Entity Entity;

        public PlayerReplicationData ReplicationData;
    }
}
