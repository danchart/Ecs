using Common.Core.Numerics;
using Ecs.Core;
using Ecs.Core.Collections;
using Game.Simulation.Core;
using System;

namespace Game.Simulation.Server
{
    public interface IWorldReplicationManager
    {
        void ApplyEntityChanges(EntityMapList<ReplicatedComponentData> entityComponents);
    }

    public sealed class WorldReplicationManager : IWorldReplicationManager
    {
        private readonly ReplicationPriorityEntityComponents _packetPriorityComponents;
        private readonly WorldPlayers _players;

        private readonly IEntityGridMap _entityGridMap;
        private readonly PacketPriorityCalculator _packetPriorityCalculator;

        private Entity[] _entityBuffer;

        public WorldReplicationManager(
            ReplicationConfig config,
            WorldPlayers players,
            IEntityGridMap entityGridMap)
        {
            this._players = players ?? throw new ArgumentNullException(nameof(players));
            this._entityGridMap = entityGridMap ?? throw new ArgumentNullException(nameof(entityGridMap));
            this._packetPriorityComponents = new ReplicationPriorityEntityComponents(config.Capacity.InitialReplicatedEntityCapacity);
            this._packetPriorityCalculator = new PacketPriorityCalculator(config.PacketPriority);
            this._entityBuffer = new Entity[256];
        }

        public void ApplyEntityChanges(EntityMapList<ReplicatedComponentData> modifiedEntityComponents)
        {
            this._packetPriorityComponents.Clear();

            // Apply changes to player entity change lists
            foreach (ref var player in this._players)
            {
                if (player.TryGetEntity(out Entity playerEntity))
                {
                    this._entityGridMap.GetEntitiesOfInterest(
                        playerEntity, 
                        ref this._entityBuffer, 
                        out int entityCount);

                    AddPacketPrioritizedEntityChangesToPlayer(
                        playerEntity,
                        this._entityBuffer,
                        entityCount,
                        modifiedEntityComponents,
                        this._packetPriorityComponents,
                        player.ReplicationData);
                }
            }
        }

        private void AddPacketPrioritizedEntityChangesToPlayer(
            in Entity playerEntity,
            Entity[] entitiesOfInterest,
            int entityCount,
            EntityMapList<ReplicatedComponentData> replicatedEntities,
            ReplicationPriorityEntityComponents packetPriorityComponents,
            PlayerReplicationData playerReplicationData)
        {
            ref readonly var playerTransform = ref playerEntity.GetReadOnlyComponent<TransformComponent>();

            for (int i = 0; i < entityCount; i++)
            {
                var entity = entitiesOfInterest[i];

                ref readonly var prioritizationComponents = ref packetPriorityComponents.GetComponents(entity);
                ref readonly var transform = ref prioritizationComponents.Transform.UnrefReadOnly();
                ref readonly var replicated = ref prioritizationComponents.Replicated.UnrefReadOnly();

                float entityPacketPriority = this._packetPriorityCalculator.GetBasePriority(replicated.BasePriority);
                entityPacketPriority *= this._packetPriorityCalculator.GetFactorFromDistance(playerTransform, transform);

                // TODO: Compute relevance based on entity size, etc.
                float entityPlayerRelevance = 1.0f;

                playerReplicationData.AddEntityChanges(
                    entity,
                    modifiedComponents: replicatedEntities[entity],
                    priority: entityPacketPriority,
                    relevance: entityPlayerRelevance);
            }
        }

        private class PacketPriorityCalculator
        {
            private readonly ReplicationConfig.PacketPriorityConfig _config;

            public PacketPriorityCalculator(ReplicationConfig.PacketPriorityConfig config)
            {
                this._config = config;
            }

            public float GetBasePriority(PriorityEnum priority)
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

            public float GetFactorFromDistance(
                in TransformComponent playerTransform,
                in TransformComponent transform)
            {
                var distSquared = Vector2.DistanceSquared(
                    transform.position,
                    playerTransform.position);

                return
                    distSquared < this._config.DistanceSquardRing0
                    ? this._config.Ring3Priority
                    : distSquared < this._config.DistanceSquardRing1
                        ? this._config.Ring1Priority
                        : distSquared < this._config.DistanceSquardRing2
                            ? this._config.Ring2Priority
                            : this._config.Ring3Priority;
            }
        }
    }
}
