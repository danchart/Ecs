using Common.Core;
using Ecs.Core;
using Ecs.Core.Collections;
using Game.Simulation.Core;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Game.Simulation.Server
{
    public class PlayerReplicationData
    {
        private ReplicatedEntity[] _replicatedEntities;
        private Dictionary<Entity, int> _entityToIndex;

        private int _count;

        private float _tickTime;
        private readonly int[] _queueTicks;

        public PlayerReplicationData(int capacity, float tickTime, int[] queueTicks)
        {
            this._replicatedEntities = new ReplicatedEntity[capacity];
            this._entityToIndex = new Dictionary<Entity, int>(capacity);
            this._count = 0;

            this._tickTime = tickTime;
            this._queueTicks = queueTicks;
        }

        public int Count => this._count;
        public ref readonly ReplicatedEntity this[int index] => ref this._replicatedEntities[index];

        public void Clear()
        {
            this._entityToIndex.Clear();
            this._count = 0;
        }

        public void Remove(Entity entity)
        {

        }

        public void AddEntityChanges(
            Entity entity,
            EntityMapList<GenerationedReplicatedComponentData>.ItemList components,
            float priority,
            float relevance)
        {
            bool wasUnassigned = false;
            int index;

            if (!this._entityToIndex.ContainsKey(entity))
            {
                wasUnassigned = true;

                if (this._count == this._replicatedEntities.Length)
                {
                    Array.Resize(ref this._replicatedEntities, 2 * _count);
                }

                index = _count++;

                this._entityToIndex[entity] = index;
            }
            else
            {
                index = this._entityToIndex[entity];
            }

            ref var entityReplicationData = ref this._replicatedEntities[index];

            // Update priority and relevance, but preserve the assigned queue time if there is one to ensure
            // this entity is dispatched at the correct time.

            entityReplicationData.Priority.Priority = Math.Min(1.0f, priority);
            entityReplicationData.Priority.Relevance = Math.Min(1.0f, relevance);
            entityReplicationData.Priority.RequestedQueueTimeRemaining =
                wasUnassigned
                // Assign new queue time.
                ? GetQueueTimeFromPriority(entityReplicationData.Priority.Priority)
                // Keep the existing queue time.
                : entityReplicationData.Priority.RequestedQueueTimeRemaining;

            foreach (var component in components)
            {
                // TODO: We must keep the PREVIOUS entity data copy to compute the delta (HasFields)

                if (entityReplicationData._components.ContainsKey(component.ComponentData.ComponentIdAsIndex))
                {
                    // Merge

                    ref readonly var t = ref entityReplicationData._components[component.ComponentData.ComponentIdAsIndex];

                    //entityReplicationData._components.Com
                }
                else
                {
                    // Add component

                    entityReplicationData._components[component.ComponentData.ComponentIdAsIndex] =
                        new ReplicatedEntity.Component
                        {
                            // Replicate all fields as there is no delta.
                            HasFields = BitField.NewSetAll(component.ComponentData.FieldCount),
                            Data = component.ComponentData
                        };
                }
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
                (int)(0.5f + this._queueTicks.Length * queuePriority));

            return this._tickTime * this._queueTicks[index];
        }

        public struct ReplicatedEntity
        {
            public QueuePriority Priority;

            public Dictionary<ComponentId, BitField> _prevReplicatedComponentFields;
            public FixedIndexDictionary<Component> _components;

            public ReplicatedEntity(int componentCapacity) : this()
            {
                this._prevReplicatedComponentFields = new Dictionary<ComponentId, BitField>(componentCapacity);
                this._components = new FixedIndexDictionary<Component>((int)ComponentId.MaxValue);
            }

            public struct QueuePriority
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

            public struct Component
            {
                public BitField HasFields;
                public ReplicatedComponentData Data;
            }
        }
    }
}
