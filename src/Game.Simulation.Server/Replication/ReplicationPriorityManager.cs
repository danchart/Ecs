using Common.Core;
using Common.Core.Numerics;
using Ecs.Core;
using Ecs.Core.Collections;
using Game.Simulation.Core;
using System;
using System.Collections.Generic;

namespace Game.Simulation.Server
{
    public interface IReplicationPriorityManager
    {
        void AddEntityChangesToPlayer(
            Entity player,
            EntityMapList<GenerationedReplicatedComponentData> replicatedEntities,
            ReplicationContext context,
            PlayerReplicationData entityPriorities);
    }

    public class ReplicationPriorityManager : IReplicationPriorityManager
    {
        private readonly ReplicationPriorityConfig _config;

        public ReplicationPriorityManager(ReplicationPriorityConfig config)
        {
            this._config = config;
        }

        public void AddEntityChangesToPlayer(
            Entity player,
            EntityMapList<GenerationedReplicatedComponentData> replicatedEntities,
            ReplicationContext context,
            PlayerReplicationData playerReplicationData)
        {
            ref readonly var playerTransform = ref player.GetReadOnlyComponent<TransformComponent>();

            foreach (var entityItem in replicatedEntities)
            {
                ref readonly var components = ref context.GetHydrated(entityItem.Entity);

                ref readonly var transform = ref components.Transform.UnrefReadOnly();
                ref readonly var replicated = ref components.Replicated.UnrefReadOnly();

                float priority = GetBasePriority(replicated.BasePriority);
                priority *= GetFactorFromDistance(playerTransform, transform);

                // TODO: Compute relevance based on entity size, etc.
                float relevance = 1.0f;

                playerReplicationData.AddEntityChanges(
                    entityItem.Entity,
                    entityItem.Items,
                    priority: priority,
                    relevance: relevance); 
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
