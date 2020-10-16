using Ecs.Core;
using Ecs.Core.Collections;
using System;

namespace Game.Simulation.Server
{
    public interface IReplicationManager
    {
        ReplicationConfig Config { get; }

        EntityMapListMOTHBALL<ReplicatedComponentData> EntityComponents { get;  }

        void Sync();
    }

    public class ReplicationManager : IReplicationManager
    {
        private readonly IPlayerConnectionManager _playerConnectionManager;

        private readonly World _world;

        private EntityMapListMOTHBALL<ReplicatedComponentData> _entityComponents;

        public ReplicationManager(
            ReplicationConfig config,
            World world,
            IPlayerConnectionManager playerConnectionManager)
        {
            this.Config = config;
            this._world = world ?? throw new ArgumentNullException(nameof(world));
            this._playerConnectionManager = playerConnectionManager ?? throw new ArgumentNullException(nameof(playerConnectionManager));

            _entityComponents = new EntityMapListMOTHBALL<ReplicatedComponentData>(
                entityCapacity: config.InitialReplicatedEntityCapacity,
                listPoolCapacity: config.InitialReplicatedEntityCapacity,
                listCapacity: config.InitialReplicatedComponentCapacity);
        }

        public ReplicationConfig Config { get; private set; }

        public EntityMapListMOTHBALL<ReplicatedComponentData> EntityComponents => _entityComponents;

        public void Sync()
        {
            // 1) Convert components to packet data
            // 2) Determinee priority per-entity for every player - O(E * P) 
            // 3) Create replication packet. Delta with last packet.

            foreach (var pair in this._playerConnectionManager.Connections)
            {
                var playerEntity = pair.Value.Entity;


            }
        }
    }
}
