using Ecs.Core;
using Game.Simulation.Core;
using System;
using System.Collections.Generic;

namespace Game.Simulation.Server
{
    public sealed class ReplicationPriorityEntityComponents
    {
        private PrioritizationComponentRefs[] _components;

        private Dictionary<Entity, int> _entityToIndex;

        private int _count;

        public ReplicationPriorityEntityComponents(int capacity)
        {
            this._components = new PrioritizationComponentRefs[capacity];
            this._entityToIndex = new Dictionary<Entity, int>(capacity);

            this._count = 0;
        }

        public void Clear()
        {
            this._entityToIndex.Clear();
            this._count = 0;
        }

        public ref readonly PrioritizationComponentRefs GetComponents(in Entity entity)
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

        private void Hydrate(in Entity entity, ref PrioritizationComponentRefs entityComponents)
        {
            entityComponents.Transform = entity.Reference<TransformComponent>();
            entityComponents.Replicated = entity.Reference<ReplicatedComponent>();
        }

        public struct PrioritizationComponentRefs
        {
            public ComponentRef<TransformComponent> Transform;
            public ComponentRef<ReplicatedComponent> Replicated;
        }
    }
}
