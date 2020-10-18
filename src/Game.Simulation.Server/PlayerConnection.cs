using Ecs.Core;

namespace Game.Simulation.Server
{
    public struct PlayerConnection
    {
        public Entity Entity;

        public PlayerReplicationData ReplicationData;

        public void Assign(in Entity entity)
        {
            this.ReplicationData.Clear();
            this.Entity = entity;
        }
    }
}
