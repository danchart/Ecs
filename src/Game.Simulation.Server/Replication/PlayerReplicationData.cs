using Common.Core;
using Ecs.Core;
using Ecs.Core.Collections;
using Game.Networking;
using Game.Simulation.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Game.Simulation.Server
{
    public sealed class PlayerReplicationData
    {
        private EntityReplicationData[] _replicatedEntities;
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
            this._replicatedEntities = new EntityReplicationData[capacity];
            this._freeIndices = new int[capacity];
            this._entityToIndex = new Dictionary<Entity, int>(capacity);
            this._count = 0;
            this._freeCount = 0;

            this._tickTime = tickTime;
            this._queueTicks = queueTicks;
        }

        public int Count => this._count;
        public ref readonly EntityReplicationData this[int index] => ref this._replicatedEntities[index];

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

                if (this._replicatedEntities[index].Components == null)
                {
                    this._replicatedEntities[index].Components = new FixedIndexDictionary<EntityReplicationData.Component>(this._componentCapacity);
                    this._replicatedEntities[index].LastReplicatedComponentFields = new Dictionary<ComponentId, BitField>(this._componentCapacity);
                }
            }
            else
            {
                index = this._entityToIndex[entity];
            }

            ref var entityReplicationData = ref this._replicatedEntities[index];

            // Update priority and relevance, but preserve the assigned queue time if there is one to ensure
            // this entity is dispatched at the correct time.

            entityReplicationData.NetPriority.Priority = Math.Min(1.0f, priority);
            entityReplicationData.NetPriority.Relevance = Math.Min(1.0f, relevance);
            entityReplicationData.NetPriority.RemainingQueueTime =
                wasUnassigned
                // Assign new queue time.
                ? GetQueueTimeFromPriority(entityReplicationData.NetPriority.Priority)
                // Keep the existing queue time.
                : entityReplicationData.NetPriority.RemainingQueueTime;

            foreach (ref var modifiedComponent in modifiedComponents)
            {
                if (entityReplicationData.Components.ContainsKey(modifiedComponent.ComponentIdAsIndex))
                {
                    // Merge component changes into the replication data.

                    ref var replicationComponentData = ref entityReplicationData.Components[modifiedComponent.ComponentIdAsIndex];

                    replicationComponentData.Merge(modifiedComponent);
                }
                else
                {
                    // Add component to the replication data.

                    entityReplicationData.Components[modifiedComponent.ComponentIdAsIndex] =
                        new EntityReplicationData.Component
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
            private readonly EntityReplicationData[] _replicatedEntities;
            private Dictionary<Entity, int>.Enumerator _enumerator;

            internal Enumerator(PlayerReplicationData replicationData)
            {
                this._replicatedEntities = replicationData._replicatedEntities;
                this._enumerator = replicationData._entityToIndex.GetEnumerator();
            }

            public ref EntityReplicationData Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref this._replicatedEntities[this._enumerator.Current.Value];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => this._enumerator.MoveNext();
        }

        public struct EntityReplicationData
        {
            public NetPriorityData NetPriority;

            public Dictionary<ComponentId, BitField> LastReplicatedComponentFields;
            public FixedIndexDictionary<Component> Components;

            public EntityReplicationData(int componentCapacity) : this()
            {
                this.LastReplicatedComponentFields = new Dictionary<ComponentId, BitField>(componentCapacity);
                this.Components = new FixedIndexDictionary<Component>((int)ComponentId.MaxValue);
            }

            public void Clear()
            {
                this.LastReplicatedComponentFields.Clear();
                this.Components.Clear();
            }

            public struct NetPriorityData
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
                public float RemainingQueueTime;
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

    public static class PlayerReplicationDataExtensions
    {
        public static int MeasurePacketSize(this PlayerReplicationData.EntityReplicationData replicationData)
        {
            int size = 0;

            for (int i = 0; i < (int)ComponentId.MaxValue; i++)
            {
                if (replicationData.Components.ContainsKey(i))
                {
                    ref var component = ref replicationData.Components[i];

                    switch (component.Data.ComponentId)
                    {
                        case ComponentId.Transform:

                            size += component.Data.Transform.Serialize(component.HasFields, Stream.Null, measureOnly: true);
                            break;

                        case ComponentId.Movement:

                            size += component.Data.Movement.Serialize(component.HasFields, Stream.Null, measureOnly: true);
                            break;

                        default:

                            throw new InvalidOperationException($"Unknown {nameof(ComponentId)}, value={component.Data.ComponentId}");
                    }
                }
            }

            return size;
        }

        public static bool ToEntityComponentPackets(
            this PlayerReplicationData.EntityReplicationData replicationData, 
            ref ComponentPacketData[] items, 
            ref byte index)
        {
            for (int i = 0; i < (int)ComponentId.MaxValue; i++)
            {
                if (replicationData.Components.ContainsKey(i))
                {
                    if (index == items.Length)
                    {
                        Array.Resize(ref items, 2 * index);
                    }

                    ref var component = ref replicationData.Components[i];

                    switch (component.Data.ComponentId)
                    {
                        case ComponentId.Transform:

                            items[index].Type = ComponentPacketData.TypeEnum.Transform;
                            items[index].Transform = component.Data.Transform;
                            break;

                        case ComponentId.Movement:

                            items[index].Type = ComponentPacketData.TypeEnum.Movement;
                            items[index].Movement = component.Data.Movement;
                            break;

                        case ComponentId.Player:

                            items[index].Type = ComponentPacketData.TypeEnum.Player;
                            items[index].Player = component.Data.Player;
                            break;

                        default:

                            throw new InvalidOperationException($"Unknown {nameof(ComponentId)}, value={component.Data.ComponentId}");
                    }

                    items[index].HasFields = component.HasFields;

                    index++;
                }
            }

            return true;
        }
    }
}
