using System;
using System.Collections.Generic;
using System.Security;

namespace Ecs.Core
{
    public abstract class EntityQuery
    {
        public int[] ComponentTypeIndices;

        private Entity[] _entities = new Entity[EcsConstants.InitialEntityQueryEntityCapacity];
        private int _entityCount = 0;

        private Dictionary<int, int> _entityIndexToQueryIndex = new Dictionary<int, int>(EcsConstants.InitialEntityQueryEntityCapacity);

        protected EntityQuery(World world)
        {
        }

        public Entity GetEntity(int index)
        {
            return _entities[index];
        }

        public int GetEntityCount()
        {
            return _entityCount;
        }

        /// <summary>
        /// Returns true if this query matches the given entity. False otherwise.
        /// </summary>
        public bool IsMatch(in World.EntityData entityData)
        {
            for (int i = 0; i < ComponentTypeIndices.Length; i++)
            {
                bool hasComponent = false;

                for (int j = 0; j < entityData.ComponentCount; j++)
                {
                    if (entityData.Components[j].TypeIndex == ComponentTypeIndices[i])
                    {
                        hasComponent = true;

                        break;
                    }
                }

                if (!hasComponent)
                {
                    return false;
                }
            }

            return true;
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

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_entities, _entityCount);
        }

        public struct Enumerator
        {
            private readonly Entity[] _entities;
            private readonly int _count;
            private int _current;

            internal Enumerator(Entity[] entities, int count)
            {
                _entities = entities ?? throw new ArgumentNullException(nameof(entities)); ;
                _count = count;
                _current = -1;
            }

            public Entity Current => _entities[_current];

            public bool MoveNext()
            {
                return ++_current < _count;
            }
        }
    }

    public class EntityQuery<T> : EntityQuery where T : struct
    {
        protected EntityQuery(World world) : base(world)
        {
            this.ComponentTypeIndices = new[] { ComponentType<T>.Index };

            //this.ComponentTypeIndices = new int[types.Length];

            //for (int i = 0; i < types.Length; i++)
            //{
            //    var componentTypeType = typeof(ComponentType<>);
            //    var type = componentTypeType.MakeGenericType(new Type[] { types[i] });
            //    var componentType = Activator.CreateInstance(type);

            //    var field = type.GetField("componentTypeIndex", BindingFlags.Static | BindingFlags.Public);

            //    var componentIndex = (int)field.GetValue(componentType);

            //    this.ComponentTypeIndices[i] = componentIndex;
            //}
        }
    }

}
