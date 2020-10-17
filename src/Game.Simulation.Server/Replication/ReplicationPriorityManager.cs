using Common.Core.Numerics;
using Ecs.Core;
using Ecs.Core.Collections;
using Game.Simulation.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Game.Simulation.Server
{
    public interface IReplicationPriorityManager
    {
        void AssignEntityPriorities(
            Entity player,
            EntityMapList<ReplicatedComponentData> replicatedEntities,
            ReplicationPriorityContext context,
            EntityReplicationPriorities entityPriorities);
    }

    public class ReplicationPriorityManager : IReplicationPriorityManager
    {
        private readonly ReplicationPriorityConfig _config;

        public ReplicationPriorityManager(ReplicationPriorityConfig config)
        {
            this._config = config;
        }

        public void AssignEntityPriorities(
            Entity player,
            EntityMapList<ReplicatedComponentData> replicatedEntities,
            ReplicationPriorityContext context,
            EntityReplicationPriorities entityPriorities)
        {
            ref readonly var playerTransform = ref player.GetReadOnlyComponent<TransformComponent>();

            foreach (var entityItem in replicatedEntities)
            {
                ref readonly var components = ref context.GetHydrated(entityItem.Entity);

                float priority = 1.0f;

                ref readonly var transform = ref components.Transform.UnrefReadOnly();
                ref readonly var replicated = ref components.Replicated.UnrefReadOnly();

                priority *= FromDistance(playerTransform, transform);
                priority *= FromBasePriority(replicated.BasePriority);

                // TODO: Compute relevance based on entity size, etc.
                float relevance = 1.0f;

                entityPriorities.Assign(
                    entityItem.Entity,
                    priority: priority,
                    relevance: relevance); 
            }
        }

        private float FromBasePriority(PriorityEnum relevance)
        {
            switch (relevance)
            {
                case PriorityEnum.Low:
                    return 0.5f;
                case PriorityEnum.Normal:
                    return 1.0f;
                case PriorityEnum.High:
                    return 2.0f;
                default:
                    throw new InvalidOperationException($"Unknown {nameof(PriorityEnum)} value={relevance}");
            }
        }

        private float FromDistance(
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

    public class ReplicationPriorityContext
    {
        private EntityRelevantComponents[] _components;

        private Dictionary<Entity, int> _entityToIndex;

        private int _count;

        public ReplicationPriorityContext(int capacity)
        {
            this._components = new EntityRelevantComponents[capacity];
            this._entityToIndex = new Dictionary<Entity, int>(capacity);

            this._count = 0;
        }

        public void Clear()
        {
            this._entityToIndex.Clear();
            this._count = 0;
        }

        public ref readonly EntityRelevantComponents GetHydrated(in Entity entity)
        {
            int index;

            if (!this._entityToIndex.ContainsKey(entity))
            {
                if (this._components.Length == this._count)
                {
                    Array.Resize(ref this._components, 2 * this._count);
                }

                this._entityToIndex[entity] = _count;

                ref var item = ref this._components[_count];

                Hydrate(entity, ref item);

                index = this._count++;

                return ref item;
            }
            else
            {
                return ref this._components[this._entityToIndex[entity]];
            }
        }

        private void Hydrate(in Entity entity, ref EntityRelevantComponents entityComponents)
        {
            entityComponents.Transform = entity.Reference<TransformComponent>();
            entityComponents.Replicated = entity.Reference<ReplicatedComponent>();
        }

        public struct EntityRelevantComponents
        {
            public ComponentRef<TransformComponent> Transform;
            public ComponentRef<ReplicatedComponent> Replicated;
        }
    }

    public class EntityReplicationPriorities
    {
        private EntityReplicationPriority[] _priorities;
        private Dictionary<Entity, int> _entityToIndex;

        private int _count;

        private float _tickTime;
        private readonly int[] _queueTicks;

        public EntityReplicationPriorities(int capacity, float tickTime, int[] queueTicks)
        {
            this._priorities = new EntityReplicationPriority[capacity];
            this._count = 0;

            this._tickTime = tickTime;
            this._queueTicks = queueTicks;
        }

        public int Count => this._count;
        public ref readonly EntityReplicationPriority this[int index] => ref this._priorities[index];

        public void Clear()
        {
            this._entityToIndex.Clear();
            this._count = 0;
        }

        public void Assign(
            Entity entity,
            float priority,
            float relevance)
        {
            bool wasUnassigned = false;
            int index;

            if (!this._entityToIndex.ContainsKey(entity))
            {
                wasUnassigned = true;

                if (this._count == this._priorities.Length)
                {
                    Array.Resize(ref this._priorities, 2 * _count);
                }

                index = _count++;

                this._entityToIndex[entity] = index;
            }
            else
            {
                index = this._entityToIndex[entity];
            }

            ref var entityPriority = ref this._priorities[index];

            // Update priority and relevance, but preserve the assigned queue time if there is one to ensure
            // this entity is dispatched at the correct time.

            entityPriority.Priority = Math.Min(1.0f, priority);
            entityPriority.Relevance = Math.Min(1.0f, relevance);
            entityPriority.RequestedQueueTimeRemaining =
                wasUnassigned
                // Assign new queue time.
                ? GetRequestedQueueTime(entityPriority.Priority, entityPriority.Relevance)
                // Keep the existing queue time.
                : entityPriority.RequestedQueueTimeRemaining;
        }

        private float GetRequestedQueueTime(float priority, float relevance)
        {
            float queuePriority = 
                Math.Max(
                    0, 
                    1.0f - (priority * relevance));
            int index = Math.Min(
                this._queueTicks.Length - 1,
                (int) (this._queueTicks.Length * queuePriority));

            return this._tickTime * this._queueTicks[index];
        }
    }

    public struct EntityReplicationPriority
    {
        /// <summary>
        /// How much does this effect gameplay. [0..1]
        /// </summary>
        public float Priority;

        /// <summary>
        /// How noticable is this entity. [0..1]
        /// </summary>
        public float Relevance;

        /// <summary>
        /// Desired update period, in seconds.
        /// </summary>
        public float RequestedQueueTimeRemaining;
    }
}
