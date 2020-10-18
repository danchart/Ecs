using Ecs.Core;

namespace Game.Simulation.Server
{
    public struct PlayerConnection
    {
        public Entity Entity;

        public PlayerReplicationData ReplicationData;

        public void Clear()
        {
            // TODO
        }
    }
}
