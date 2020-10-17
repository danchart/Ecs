﻿using Ecs.Core;
using Ecs.Core.Collections;
using System;

namespace Game.Simulation.Server
{
    public interface IReplicationManager
    {
        ReplicationConfig Config { get; }

        EntityMapList<ReplicatedComponentData> EntityComponents { get;  }

        void Sync();
    }

    public class ReplicationManager : IReplicationManager
    {
        private readonly IPlayerConnectionManager _playerConnectionManager;
        private readonly IReplicationPriorityManager _priorityManager;

        private readonly World _world;

        private readonly EntityMapList<ReplicatedComponentData> _entityComponents;

        private readonly EntityReplicationPriorities _entityPriorities;

        private readonly ReplicationPriorityContext _context;

        public ReplicationManager(
            ReplicationConfig config,
            World world,
            IPlayerConnectionManager playerConnectionManager)
        {
            this.Config = config;
            this._world = world ?? throw new ArgumentNullException(nameof(world));
            this._playerConnectionManager = playerConnectionManager ?? throw new ArgumentNullException(nameof(playerConnectionManager));

            _entityComponents = new EntityMapList<ReplicatedComponentData>(
                entityCapacity: config.InitialReplicatedEntityCapacity,
                listCapacity: config.InitialReplicatedComponentCapacity);

            _context = new ReplicationPriorityContext(config.InitialReplicatedEntityCapacity);
        }

        public ReplicationConfig Config { get; private set; }

        public EntityMapList<ReplicatedComponentData> EntityComponents => _entityComponents;

        public void Sync()
        {
            // 1) Convert components to packet data
            // 2) Determinee priority per-entity for every player - O(E * P) 
            // 3) Create replication packet. Delta with last packet.

            foreach (var pair in this._playerConnectionManager.Connections)
            {
                var playerEntity = pair.Value.Entity;

                _priorityManager.AssignEntityPriorities(
                    playerEntity, 
                    _entityComponents, 
                    _context, 
                    pair.Value.EntityPriorities);
            }
        }
    }
}
