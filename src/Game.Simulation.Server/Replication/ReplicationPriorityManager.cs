using Ecs.Core;
using Ecs.Core.Collections;
using Game.Simulation.Core;
using System;
using System.Collections.Generic;

namespace Game.Simulation.Server
{
    public interface IReplicationPriorityManager
    {
        ReplicationPriority[] GetPriorities(
            Entity player,
            EntityMapList<ReplicatedComponentData> replicatedEntities,
            ReplicationPriorityContext context);
    }

    public class ReplicationPriorityManager : IReplicationPriorityManager
    {
        private readonly ReplicationPriorityConfig _config;

        public ReplicationPriorityManager(ReplicationPriorityConfig config)
        {
            this._config = config;
        }

        public ReplicationPriority[] GetPriorities(
            Entity player,
            EntityMapList<ReplicatedComponentData> replicatedEntities,
            ReplicationPriorityContext context)
        {
            ref readonly var playerTransform = ref player.GetReadOnlyComponent<TransformComponent>();

            foreach (var entityItem in replicatedEntities)
            {
                ref readonly var data = ref context.GetHydrated(entityItem.Entity);

                //ref readonly var transform = ref entityItem.Entity.GetReadOnlyComponent<TransformComponent>();

                ref readonly var transform = ref data.Transform.UnrefReadOnly();


            }

            return new ReplicationPriority[0];
        }

        private static float FromDistance(
            in TransformComponent playerTransform,
            in TransformComponent transform)
        {
            //var distSquared = transform.

            return 1.0f;
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
        }

        public struct EntityRelevantComponents
        {
            public ComponentRef<TransformComponent> Transform;
        }
    }

    public struct ReplicationPriority
    {
        /// <summary>
        /// The final computed priority. [0..1]
        /// </summary>
        public float FinalPriority;

        /// <summary>
        /// The player perceived relevance or visibility. [0..1]
        /// </summary>
        public float Relevance;

        /// <summary>
        /// Desired update period, in milliseconds.
        /// </summary>
        public float UpdatePeriod;
    }
}
