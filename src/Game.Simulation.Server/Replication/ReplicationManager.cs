﻿using Common.Core.Numerics;
using Ecs.Core;
using Ecs.Core.Collections;
using Game.Simulation.Core;
using System;

namespace Game.Simulation.Server
{
    public interface IReplicationManager
    {
        void Apply(EntityMapList<ReplicatedComponentData> entityComponents);
    }

    public class ReplicationManager : IReplicationManager
    {
        private readonly ReplicationConfig _config;

        private readonly ReplicationPacketPriorityComponents _packetPriorityComponents;
        private readonly PlayerConnections _playerConnections;

        public ReplicationManager(
            ReplicationConfig config,
            PlayerConnections playerConnectionManager)
        {
            this._config = config;
            this._playerConnections = playerConnectionManager;
            this._packetPriorityComponents = new ReplicationPacketPriorityComponents(config.InitialReplicatedEntityCapacity);
        }

        public void Apply(EntityMapList<ReplicatedComponentData> modifiedEntityComponents)
        {
            this._packetPriorityComponents.Clear();

            // Apply changes to player entity change lists
            for (int i = 0; i < this._playerConnections.Count; i++)
            {
                ref var connection = ref this._playerConnections[i];

                var playerEntity = connection.Entity;

                AddPacketPrioritizedEntityChangesToPlayer(
                    playerEntity,
                    modifiedEntityComponents,
                    this._packetPriorityComponents,
                    connection.ReplicationData);
            }
        }

        private void AddPacketPrioritizedEntityChangesToPlayer(
            in Entity playerEntity,
            EntityMapList<ReplicatedComponentData> replicatedEntities,
            ReplicationPacketPriorityComponents packetPriorityComponents,
            PlayerReplicationData playerReplicationData)
        {
            ref readonly var playerTransform = ref playerEntity.GetReadOnlyComponent<TransformComponent>();

            foreach (var replicatedEntityComponents in replicatedEntities)
            {
                ref readonly var components = ref packetPriorityComponents.GetComponents(replicatedEntityComponents.Entity);

                ref readonly var transform = ref components.Transform.UnrefReadOnly();
                ref readonly var replicated = ref components.Replicated.UnrefReadOnly();

                float entityPacketPriority = GetBasePriority(replicated.BasePriority);
                entityPacketPriority *= GetFactorFromDistance(playerTransform, transform);

                // TODO: Compute relevance based on entity size, etc.
                float entityPlayerRelevance = 1.0f;

                playerReplicationData.AddEntityChanges(
                    replicatedEntityComponents.Entity,
                    replicatedEntityComponents.Items,
                    priority: entityPacketPriority,
                    relevance: entityPlayerRelevance); 
            }
        }

        private float GetBasePriority(PriorityEnum priority)
        {
            switch (priority)
            {
                case PriorityEnum.Low:
                    return 0.5f;
                case PriorityEnum.Normal:
                    return 1.0f;
                case PriorityEnum.High:
                    return 2.0f;
                default:
                    throw new InvalidOperationException($"Unknown {nameof(PriorityEnum)} value={priority}");
            }
        }

        private float GetFactorFromDistance(
            in TransformComponent playerTransform,
            in TransformComponent transform)
        {
            var distSquared = Vector2.DistanceSquared(
                transform.position,
                playerTransform.position);

            return
                distSquared < _config.DistanceSquardRing0
                ? _config.Ring3Priority
                : distSquared < _config.DistanceSquardRing1
                    ? _config.Ring1Priority
                    : distSquared < _config.DistanceSquardRing2
                        ? _config.Ring2Priority
                        : _config.Ring3Priority;

        }
    }
}
