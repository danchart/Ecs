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
        void AssignPlayersEntityPriorities(
            Entity player,
            EntityMapList<ReplicatedComponentData> replicatedEntities,
            ReplicationContext context,
            EntityPriorities entityPriorities);
    }

    public class ReplicationPriorityManager : IReplicationPriorityManager
    {
        private readonly ReplicationPriorityConfig _config;

        public ReplicationPriorityManager(ReplicationPriorityConfig config)
        {
            this._config = config;
        }

        public void AssignPlayersEntityPriorities(
            Entity player,
            EntityMapList<ReplicatedComponentData> replicatedEntities,
            ReplicationContext context,
            EntityPriorities entityPriorities)
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

                entityPriorities.Assign(
                    entityItem.Entity,
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

    public class EntityPriorities
    {
        private EntityPriority[] _priorities;
        private Dictionary<Entity, int> _entityToIndex;

        private int _count;

        private float _tickTime;
        private readonly int[] _queueTicks;

        public EntityPriorities(int capacity, float tickTime, int[] queueTicks)
        {
            this._priorities = new EntityPriority[capacity];
            this._entityToIndex = new Dictionary<Entity, int>(capacity);
            this._count = 0;

            this._tickTime = tickTime;
            this._queueTicks = queueTicks;
        }

        public int Count => this._count;
        public ref readonly EntityPriority this[int index] => ref this._priorities[index];

        public void Clear()
        {
            this._entityToIndex.Clear();
            this._count = 0;
        }

        public void Remove(Entity entity)
        {

        }

        public void Assign(
            Entity entity,
            EntityMapList<ReplicatedComponentData>.ItemList components,
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
                ? GetQueueTimeFromPriority(entityPriority.Priority)
                // Keep the existing queue time.
                : entityPriority.RequestedQueueTimeRemaining;

            foreach (var component in components)
            {

            }
        }

        private float GetQueueTimeFromPriority(float priority)
        {
            float queuePriority = 
                Math.Max(
                    0, 
                    1.0f - priority);
            int index = Math.Min(
                this._queueTicks.Length - 1,
                (int) (0.5f + this._queueTicks.Length * queuePriority));

            return this._tickTime * this._queueTicks[index];
        }
    }

    public struct EntityPriority
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

        public Dictionary<ComponentId, Component> _components;

        public struct Component
        {
            public BitField HasFields;
            public ReplicatedComponentData Data;
        }
    }
}
