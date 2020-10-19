using Common.Core.Numerics;
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

        private readonly ReplicationContext _context;
        private readonly PlayerConnections _playerConnections;

        public ReplicationManager(
            ReplicationConfig config,
            PlayerConnections playerConnectionManager)
        {
            this._config = config;
            this._playerConnections = playerConnectionManager;
            this._context = new ReplicationContext(config.InitialReplicatedEntityCapacity);
        }

        public void Apply(EntityMapList<ReplicatedComponentData> modifiedEntityComponents)
        {
            this._context.Clear();

            // Apply changes to player entity change lists
            for (int i = 0; i < this._playerConnections.Count; i++)
            {
                ref var connection = ref this._playerConnections[i];

                var playerEntity = connection.Entity;

                AddEntityChangesToPlayer(
                    playerEntity,
                    modifiedEntityComponents,
                    this._context,
                    connection.ReplicationData);
            }
        }

        private void AddEntityChangesToPlayer(
            in Entity player,
            EntityMapList<ReplicatedComponentData> replicatedEntities,
            ReplicationContext context,
            PlayerReplicationData playerReplicationData)
        {
            ref readonly var playerTransform = ref player.GetReadOnlyComponent<TransformComponent>();

            foreach (var entityItem in replicatedEntities)
            {
                ref readonly var components = ref context.GetComponents(entityItem.Entity);

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
