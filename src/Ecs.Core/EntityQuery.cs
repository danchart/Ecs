using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ecs.Core
{
    public class EntityQuery
    {
        public int[] ComponentTypeIndices;

        public World World;

        private Entity[] _entities = new Entity[EcsConstants.InitialEntityQueryEntityCapacity];
        private int _entityCount = 0;

        private Dictionary<int, int> _entityIndexToQueryIndex = new Dictionary<int, int>(EcsConstants.InitialEntityQueryEntityCapacity);

        public EntityQuery(Type[] types)
            {
            this.ComponentTypeIndices = new int[types.Length];

            for (int i = 0; i < types.Length; i++)
            {
                var componentTypeType = typeof(ComponentType<>);
                var type = componentTypeType.MakeGenericType(new Type[] { types[i] });
                var componentType = Activator.CreateInstance(type);

                var field = type.GetField("ComponentPoolIndex", BindingFlags.Static | BindingFlags.Public);

                var componentIndex = (int)field.GetValue(componentType);

                this.ComponentTypeIndices[i] = componentIndex;
            }
        }

        public Entity GetEntity(int index)
        {
            return _entities[index];
        }

        public int GetEntityCount()
        {
            return _entityCount;
        }

        public void AddEntity(in Entity entity)
        {
            if (_entities.Length == _entityCount)
            {
                Array.Resize(ref _entities, 2 * _entityCount);
            }

            _entityIndexToQueryIndex[entity.Id] = _entityCount;
            _entities[_entityCount++] = entity;
        }

        public void RemoveEntity(in Entity entity)
        {
            var queryEntityIndex = _entityIndexToQueryIndex[entity.Id];

            // Move the last item to removed position.
            if (queryEntityIndex <_entityCount - 1)
            {
                _entities[queryEntityIndex] = _entities[_entityCount - 1];
            }

            _entityIndexToQueryIndex.Remove(entity.Id);

            _entityCount--;
        }
    }
}
