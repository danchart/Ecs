using Ecs.Core;
using Ecs.Core.Collections;
using System;

namespace Game.Simulation.Server
{
    public interface IReplicationManager
    {
        EntityMapList<ReplicatedComponentData> BeginDataCollection();
        void EndDataCollection();
    }

    public class ReplicationManager : IReplicationManager
    {
        private readonly IPlayerConnectionManager _playerConnectionManager;
        private readonly IReplicationPriorityManager _priorityManager;

        public readonly ReplicationConfig _config;
        private readonly World _world;

        private readonly EntityMapList<ReplicatedComponentData> _entityComponents;
        private readonly ReplicationContext _context;

        public ReplicationManager(
            ReplicationConfig config,
            World world,
            IPlayerConnectionManager playerConnectionManager)
        {
            this._config = config;
            this._world = world ?? throw new ArgumentNullException(nameof(world));
            this._playerConnectionManager = playerConnectionManager ?? throw new ArgumentNullException(nameof(playerConnectionManager));

            _entityComponents = new EntityMapList<ReplicatedComponentData>(
                entityCapacity: config.InitialReplicatedEntityCapacity,
                listCapacity: config.InitialReplicatedComponentCapacity);

            _context = new ReplicationContext(config.InitialReplicatedEntityCapacity);
        }

        public EntityMapList<ReplicatedComponentData> BeginDataCollection()
        {
            return _entityComponents;
        }

        public void EndDataCollection()
        {
            // 1) Convert components to packet data
            // 2) Determinee priority per-entity for every player - O(E * P) 
            // 3) Create replication packet. Delta with last packet.

            foreach (var pair in this._playerConnectionManager.Connections)
            {
                var playerEntity = pair.Value.Entity;

                _priorityManager.AssignPlayersEntityPriorities(
                    playerEntity, 
                    _entityComponents, 
                    _context, 
                    pair.Value.EntityPriorities);
            }
        }
    }
}
