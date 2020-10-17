using Ecs.Core;
using Ecs.Core.Collections;
using System;

namespace Game.Simulation.Server
{
    public interface IReplicationManager
    {
        uint Version { get; }

        EntityMapList<GenerationedReplicatedComponentData> BeginDataCollection();
        void EndDataCollection();
    }

    public class ReplicationManager : IReplicationManager
    {
        private readonly IPlayerConnectionManager _playerConnectionManager;
        private readonly IReplicationPriorityManager _priorityManager;

        public readonly ReplicationConfig _config;
        private readonly World _world;

        private readonly EntityMapList<GenerationedReplicatedComponentData> _entityComponents;
        private readonly ReplicationContext _context;

        // Incremented every time we begin collection.
        private uint _version;

        public uint Version => _version;

        public ReplicationManager(
            ReplicationConfig config,
            World world,
            IPlayerConnectionManager playerConnectionManager)
        {
            this._config = config;
            this._world = world ?? throw new ArgumentNullException(nameof(world));
            this._playerConnectionManager = playerConnectionManager ?? throw new ArgumentNullException(nameof(playerConnectionManager));

            this._entityComponents = new EntityMapList<GenerationedReplicatedComponentData>(
                entityCapacity: config.InitialReplicatedEntityCapacity,
                listCapacity: config.InitialReplicatedComponentCapacity);

            this._context = new ReplicationContext(config.InitialReplicatedEntityCapacity);

            this._version = 0;
        }

        public EntityMapList<GenerationedReplicatedComponentData> BeginDataCollection()
        {
            // Invalidate any previously collected data.
            this._version++;
            return this._entityComponents;
        }

        public void EndDataCollection()
        {
            // 1) Convert components to packet data
            // 2) Determinee priority per-entity for every player - O(E * P) 
            // 3) Create replication packet. Delta with last packet.

            foreach (var pair in this._playerConnectionManager.Connections)
            {
                var playerEntity = pair.Value.Entity;

                _priorityManager.AddEntityChangesToPlayer(
                    playerEntity, 
                    _entityComponents, 
                    _context, 
                    pair.Value.EntityPriorities);
            }
        }
    }

    public struct GenerationedReplicatedComponentData
    {
        public uint Version;
        public ReplicatedComponentData ComponentData;
    }
}
