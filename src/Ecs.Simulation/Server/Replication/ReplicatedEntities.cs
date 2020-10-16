using Ecs.Core;
using System;
using System.Collections.Generic;

namespace Ecs.Simulation.Server
{
    public class ReplicatedEntities
    {
        private Dictionary<Entity, AppendOnlyList<ReplicatedComponentData>>[] _entityComponents;

        private Dictionary<Entity, AppendOnlyList<ReplicatedComponentData>> _current;
        private Dictionary<Entity, AppendOnlyList<ReplicatedComponentData>> _last;

        private readonly int _componentCapacity;

        public ReplicatedEntities(
            int entityCapacity,
            int componentCapacity)
        {
            _entityComponents = new Dictionary<Entity, AppendOnlyList<ReplicatedComponentData>>[2];

            _entityComponents[0] = new Dictionary<Entity, AppendOnlyList<ReplicatedComponentData>>(entityCapacity);
            _entityComponents[1] = new Dictionary<Entity, AppendOnlyList<ReplicatedComponentData>>(entityCapacity);

            _current = this._entityComponents[0];
            _last = this._entityComponents[1];

            _componentCapacity = componentCapacity;
        }

        public void Swap()
        {
            var _temp = _current;

            _current = _last;
            _last = _temp;

            _current.Clear();
        }

        public void AddEntity(Entity entity)
        {
            AppendOnlyList<ReplicatedComponentData> value;

            if (!_current.ContainsKey(entity))
            {
                if (_last.ContainsKey(entity))
                {
                    value = _last[entity];

                    value.Clear();
                }
                else
                {
                    value = new AppendOnlyList<ReplicatedComponentData>(_componentCapacity);
                }

                _current[entity] = value;
            }
        }

        public void AddComponentData(Entity entity, ReplicatedComponentData componentData)
        {
            if (!_current.ContainsKey(entity))
            {
                throw new InvalidOperationException($"Entity has not been added. Entity={entity}");
            }

            _current[entity].Add(componentData);
        }
    }
}
