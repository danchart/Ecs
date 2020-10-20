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

    public class WorldReplicationManager : IWorldReplicationManager
    {
        private readonly ReplicationPacketPriorityComponents _packetPriorityComponents;
        private readonly WorldPlayers _players;

        private readonly PacketPriorityCalculator _packetPriorityCalculator;

        public WorldReplicationManager(
            ReplicationConfig config,
            WorldPlayers players)
        {
            this._players = players ?? throw new ArgumentNullException(nameof(players));
            this._packetPriorityComponents = new ReplicationPacketPriorityComponents(config.Capacity.InitialReplicatedEntityCapacity);
            this._packetPriorityCalculator = new PacketPriorityCalculator(config.PacketPriority);
        }

        public void ApplyEntityChanges(EntityMapList<ReplicatedComponentData> modifiedEntityComponents)
        {
            this._packetPriorityComponents.Clear();

            // Apply changes to player entity change lists
            foreach (ref var player in this._players)
            {
                AddPacketPrioritizedEntityChangesToPlayer(
                    player.Entity,
                    modifiedEntityComponents,
                    this._packetPriorityComponents,
                    player.ReplicationData);
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

                float entityPacketPriority = this._packetPriorityCalculator.GetBasePriority(replicated.BasePriority);
                entityPacketPriority *= this._packetPriorityCalculator.GetFactorFromDistance(playerTransform, transform);

                // TODO: Compute relevance based on entity size, etc.
                float entityPlayerRelevance = 1.0f;

                playerReplicationData.AddEntityChanges(
                    replicatedEntityComponents.Entity,
                    replicatedEntityComponents.Items,
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
}
