using Ecs.Core;
using Game.Networking;
using System;

namespace Game.Simulation.Server
{
    public struct WorldPlayer
    {
        public PlayerConnectionRef ConnectionRef;

        public Entity Entity;

        public int PlayerReplicationDataIndex;

        private readonly PlayerReplicationDataPool _replicationDataPool;

        internal WorldPlayer(PlayerReplicationDataPool replicationDataPool) : this()
        {
            this._replicationDataPool = replicationDataPool ?? throw new ArgumentNullException(nameof(replicationDataPool));
        }

        public PlayerReplicationData ReplicationData
        {
            get => this._replicationDataPool.GetItem(PlayerReplicationDataIndex);
        }
    }
}
