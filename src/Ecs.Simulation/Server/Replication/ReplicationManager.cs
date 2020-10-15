using Ecs.Core;
using System;

namespace Ecs.Simulation.Server
{
    public interface IReplicationManager
    {
        ReplicationConfig Config { get; }

        void Sync(AppendOnlyList<AppendOnlyList<ReplicatedComponentData>> replicatedData);
    }

    public class ReplicationManager : IReplicationManager
    {
        private readonly IPlayerConnectionManager PlayerConnectionManager;

        private readonly World World;

        private readonly AppendOnlyList<ReplicatedEntityData> _replicatedEntityData;

        public ReplicationManager(
            ReplicationConfig config,
            World world,
            IPlayerConnectionManager playerConnectionManager)
        {
            this.Config = config;
            this.World = world ?? throw new ArgumentNullException(nameof(world));
            this.PlayerConnectionManager = playerConnectionManager ?? throw new ArgumentNullException(nameof(playerConnectionManager));

            _replicatedEntityData = new AppendOnlyList<ReplicatedEntityData>()
        }

        public ReplicationConfig Config { get; private set; }

        public void Sync(AppendOnlyList<AppendOnlyList<ReplicatedComponentData>> replicatedData)
        {
            // 1) Determinee priority per-entity for every player - O(E * P) 
            // 2) Create replication packet. Delta with last packet.

            foreach (var pair in this.PlayerConnectionManager.Connections)
            {
                var playerEntity = pair.Value.Entity;


            }
        }
    }
}
