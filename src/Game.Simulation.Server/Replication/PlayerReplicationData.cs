using Common.Core;
using Ecs.Core;
using Ecs.Core.Collections;
using Game.Simulation.Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.Simulation.Server
{
    public class PlayerReplicationData
    {
        private ReplicatedEntity[] _replicatedEntities;
        private Dictionary<Entity, int> _entityToIndex;

        private int[] _freeIndices;

        private int _count;
        private int _freeCount;

        private float _tickTime;
        private readonly int[] _queueTicks;

        private readonly int _componentCapacity;

        public PlayerReplicationData(int capacity, int componentCapacity, float tickTime, int[] queueTicks)
        {
            this._componentCapacity = componentCapacity;
            this._replicatedEntities = new ReplicatedEntity[capacity];
            this._freeIndices = new int[capacity];
            this._entityToIndex = new Dictionary<Entity, int>(capacity);
            this._count = 0;
            this._freeCount = 0;

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
            // Clear entity data & remove from the index.
            var index = this._entityToIndex[entity];

            this._replicatedEntities[index].Clear();
            this._entityToIndex.Remove(entity);

            // Return index to the free pool.
            this._freeIndices[this._freeCount++] = index;
        }

        public void AddEntityChanges(
            Entity entity,
            EntityMapList<ReplicatedComponentData>.ItemList modifiedComponents,
            float priority,
            float relevance)
        {
            bool wasUnassigned = false;
            int index;

            if (!this._entityToIndex.ContainsKey(entity))
            {
                wasUnassigned = true;

                if (this._freeCount > 0)
                {
                    // Use index from the free pool first.
                    index = this._freeIndices[--this._freeCount];
                }
                else
                {
                    if (this._count == this._replicatedEntities.Length)
                    {
                        Array.Resize(ref this._replicatedEntities, 2 * this._count);
                        Array.Resize(ref this._freeIndices, 2 * this._count);
                    }

                    index = this._count++;
                }

                this._entityToIndex[entity] = index;

                if (this._replicatedEntities[index]._components == null)
                {
                    this._replicatedEntities[index]._components = new FixedIndexDictionary<ReplicatedEntity.Component>(this._componentCapacity);
                    this._replicatedEntities[index]._prevReplicatedComponentFields = new Dictionary<ComponentId, BitField>(this._componentCapacity);
                }
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

            foreach (ref var modifiedComponent in modifiedComponents)
            {
                if (entityReplicationData._components.ContainsKey(modifiedComponent.ComponentIdAsIndex))
                {
                    // Merge component changes into the replication data.

                    ref var replicationComponentData = ref entityReplicationData._components[modifiedComponent.ComponentIdAsIndex];

                    replicationComponentData.Merge(modifiedComponent);
                }
                else
                {
                    // Add component to the replication data.

                    entityReplicationData._components[modifiedComponent.ComponentIdAsIndex] =
                        new ReplicatedEntity.Component
                        {
                            // Replicate all fields
                            HasFields = BitField.NewSetAll(),
                            // Copy data
                            Data = modifiedComponent
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

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator
        {
            private readonly ReplicatedEntity[] _replicatedEntities;
            private Dictionary<Entity, int>.Enumerator _enumerator;

            internal Enumerator(PlayerReplicationData replicationData)
            {
                this._replicatedEntities = replicationData._replicatedEntities;
                this._enumerator = replicationData._entityToIndex.GetEnumerator();
            }

            public ref ReplicatedEntity Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref this._replicatedEntities[this._enumerator.Current.Value];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => this._enumerator.MoveNext();
        }

        public struct ReplicatedEntity
        {
            public QueuePriority Priority;

            public Dictionary<ComponentId , BitField> _prevReplicatedComponentFields;
            public FixedIndexDictionary<Component> _components;

            public ReplicatedEntity(int componentCapacity) : this()
            {
                this._prevReplicatedComponentFields = new Dictionary<ComponentId, BitField>(componentCapacity);
                this._components = new FixedIndexDictionary<Component>((int)ComponentId.MaxValue);
            }

            public void Clear()
            {
                this._prevReplicatedComponentFields.Clear();
                this._components.Clear();
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

                public void Merge(in ReplicatedComponentData modifiedData)
                {
                    this.Data.Merge(modifiedData, ref this.HasFields);
                }
            }
        }
    }
}
