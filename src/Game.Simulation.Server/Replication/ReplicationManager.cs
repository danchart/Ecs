using Ecs.Core;
using System;

namespace Game.Simulation.Server
{
    public interface IReplicationManager
    {
        ReplicationConfig Config { get; }

        ReplicatedEntities EntityComponents { get;  }

        void Sync();
    }

    public class ReplicationManager : IReplicationManager
    {
        private readonly IPlayerConnectionManager _playerConnectionManager;

        private readonly World _world;

        private ReplicatedEntities _entityComponents;

        public ReplicationManager(
            ReplicationConfig config,
            World world,
            IPlayerConnectionManager playerConnectionManager)
        {
            this.Config = config;
            this._world = world ?? throw new ArgumentNullException(nameof(world));
            this._playerConnectionManager = playerConnectionManager ?? throw new ArgumentNullException(nameof(playerConnectionManager));

            _entityComponents = new ReplicatedEntities(
                entityCapacity: config.InitialReplicatedEntityCapacity,
                componentCapacity: config.InitialReplicatedComponentCapacity);
        }

        public ReplicationConfig Config { get; private set; }

        public ReplicatedEntities EntityComponents => _entityComponents;

        public void Sync()
        {
            // 1) Determinee priority per-entity for every player - O(E * P) 
            // 2) Create replication packet. Delta with last packet.

            foreach (var pair in this._playerConnectionManager.Connections)
            {
                var playerEntity = pair.Value.Entity;


            }
        }
    }
}
