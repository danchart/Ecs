using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Ecs.Core
{
    /// <summary>
    /// Contains all ECS state.
    /// </summary>
    public class World
    {
        internal readonly EcsConfig Config;

        internal Version GlobalSystemVersion;
        internal Version LastSystemVersion;

        internal IComponentPool[] ComponentPools;

        private EntityData[] _entities;
        private int[] _freeEntityIds;

        private int _entityCount = 0;
        private int _freeEntityCount = 0;

        private readonly Dictionary<int, AppendOnlyList<EntityQuery>> _includedComponentIdToEntityQueries;
        private readonly Dictionary<int, AppendOnlyList<EntityQuery>> _excludedComponentIdToEntityQueries;

        private readonly AppendOnlyList<EntityQuery> _queries;

        public World(EcsConfig config)
        {
            Config = config; 

            ComponentPools = new IComponentPool[Config.InitialComponentPoolCapacity];
            _entities = new EntityData[Config.InitialEntityPoolCapacity];
            _freeEntityIds = new int[Config.InitialEntityPoolCapacity];

            _includedComponentIdToEntityQueries = new Dictionary<int, AppendOnlyList<EntityQuery>>(Config.InitialComponentToEntityQueryMapCapacity);
            _excludedComponentIdToEntityQueries = new Dictionary<int, AppendOnlyList<EntityQuery>>(Config.InitialComponentToEntityQueryMapCapacity);
            _queries = new AppendOnlyList<EntityQuery>(Config.InitialEntityQueryCapacity);

            GlobalSystemVersion = new Version();
            LastSystemVersion = GlobalSystemVersion;
        }

        public Entity NewEntity()
        {
            var entity = new Entity
            {
                World = this
            };

            if (_freeEntityCount > 0)
            {
                entity.Id = _freeEntityIds[--_freeEntityCount];

                ref var entityData = ref _entities[entity.Id];
                entity.Generation = entityData.Generation;
                entityData.ComponentCount = 0;
            }
            else
            {
                entity.Id = GetNextEntityId();

                ref var entityData = ref _entities[entity.Id];

                entityData.ComponentCount = 0;
                entityData.Components = new EntityData.ComponentData[Config.InitialEntityComponentCapacity];
                entityData.Generation = EcsConstants.InitialEntityVersion;
                entity.Generation = entityData.Generation;
            }

            return entity;
        }

        public void FreeEntityData(int id, ref EntityData entityData)
        {
            entityData.ComponentCount = 0;
            entityData.Generation++;
            _freeEntityIds[_freeEntityCount++] = id;
        }

        public ref EntityData GetEntityData(in Entity entity)
        {
            return ref _entities[entity.Id];
        }

        public bool IsFreed(in Entity entity)
        {
            ref var entityData = ref _entities[entity.Id];

            return
                entityData.Generation != entity.Generation ||
                entityData.ComponentCount == 0;
        }

        public EntityQuery GetEntityQuery<T>()
        {
            return GetEntityQuery(typeof(T));
        }

        /// <summary>
        /// Returns a shared entity query of the matching type.
        /// </summary>
        public EntityQuery GetEntityQuery(Type entityQueryType)
        {
            for (int i = 0; i < _queries.Count; i++)
            {
                if (_queries.Items[i].GetType() == entityQueryType)
                {
                    // Matching query exists.
                    return _queries.Items[i];
                }
            }

            // Create query.
            var entityQuery = (EntityQuery) Activator.CreateInstance(
                entityQueryType, 
                BindingFlags.NonPublic | BindingFlags.Instance, 
                null,
                new[] { this }, 
                CultureInfo.InvariantCulture);

            _queries.Add(entityQuery);

            // Add to included component->query list
            for (int i = 0; i < entityQuery.IncludedComponentTypeIndices.Length; i++)
            {
                if (!_includedComponentIdToEntityQueries.ContainsKey(entityQuery.IncludedComponentTypeIndices[i]))
                {
                    _includedComponentIdToEntityQueries[entityQuery.IncludedComponentTypeIndices[i]] = new AppendOnlyList<EntityQuery>(EcsConstants.InitialEntityQueryEntityCapacity);
                }

                _includedComponentIdToEntityQueries[entityQuery.IncludedComponentTypeIndices[i]].Add(entityQuery);
            }

            if (entityQuery.ExcludedComponentTypeIndices != null)
            {
                // Add any to excluded component->query list
                for (int i = 0; i < entityQuery.ExcludedComponentTypeIndices.Length; i++)
                {
                    if (!_excludedComponentIdToEntityQueries.ContainsKey(entityQuery.ExcludedComponentTypeIndices[i]))
                    {
                        _excludedComponentIdToEntityQueries[entityQuery.ExcludedComponentTypeIndices[i]] = new AppendOnlyList<EntityQuery>(EcsConstants.InitialEntityQueryEntityCapacity);
                    }

                    _excludedComponentIdToEntityQueries[entityQuery.ExcludedComponentTypeIndices[i]].Add(entityQuery);
                }
            }

            return entityQuery;
        }

        public void OnAddComponent(
            int componentTypeIndex, 
            in Entity entity, 
            in EntityData entityData)
        {
            if (_includedComponentIdToEntityQueries.TryGetValue(componentTypeIndex, out var includeEntityQueries))
            {
                for (int i = 0; i < includeEntityQueries.Count; i++)
                {
                    var entityQuery = includeEntityQueries.Items[i];

                    entityQuery.OnAddIncludeComponent(entity, entityData, componentTypeIndex);
                }
            }

            if (_excludedComponentIdToEntityQueries.TryGetValue(componentTypeIndex, out var excludeEntityQueries))
            {
                for (int i = 0; i < excludeEntityQueries.Count; i++)
                {
                    var entityQuery = excludeEntityQueries.Items[i];

                    entityQuery.OnAddExcludeComponent(entity, entityData, componentTypeIndex);
                }
            }
        }

        public void OnChangeComponent(
            int componentTypeIndex,
            in Entity entity, 
            in EntityData entityData)
        {
            if (_includedComponentIdToEntityQueries.TryGetValue(componentTypeIndex, out var entityQueries))
            {
                for (int i = 0; i < entityQueries.Count; i++)
                {
                    var entityQuery = entityQueries.Items[i];

                    entityQuery.OnChangeIncludeComponent(entity, entityData, componentTypeIndex);
                }
            }

            // Changes do not effect exclusions.
        }

        public void OnRemoveComponent(
            int componentTypeIndex,
            in Entity entity,
            in EntityData entityData)
        {
            if (_includedComponentIdToEntityQueries.TryGetValue(componentTypeIndex, out var includeEntityQueries))
            {
                for (int i = 0; i < includeEntityQueries.Count; i++)
                {
                    var entityQuery = includeEntityQueries.Items[i];

                    entityQuery.OnRemoveIncludeComponent(entity, entityData, componentTypeIndex);
                }
            }

            if (_excludedComponentIdToEntityQueries.TryGetValue(componentTypeIndex, out var excludeEntityQueries))
            {
                for (int i = 0; i < excludeEntityQueries.Count; i++)
                {
                    var entityQuery = excludeEntityQueries.Items[i];

                    entityQuery.OnRemoveExcludeComponent(entity, entityData, componentTypeIndex);
                }
            }
        }

        internal ComponentPool<T> GetPool<T>() where T : struct
        {
            var poolIndex = ComponentType<T>.Index;

            if (ComponentPools.Length < poolIndex)
            {
                var len = ComponentPools.Length * 2;

                while (len <= poolIndex)
                {
                    len *= 2;
                }

                Array.Resize(ref ComponentPools, len);
            }

            var pool = (ComponentPool<T>)ComponentPools[poolIndex];

            if (pool == null)
            {
                pool = new ComponentPool<T>();
                ComponentPools[poolIndex] = pool;
            }

            return pool;
        }

        private int GetNextEntityId()
        {
            if (_freeEntityCount > 0)
            {
                return _freeEntityIds[_freeEntityCount--];
            }

            if (_entityCount == _entities.Length)
            {
                Array.Resize(ref _entities, _entityCount * 2);
            }

            return _entityCount++;
        }


        public struct EntityData
        {
            public ComponentData[] Components;
            public int ComponentCount;
            public uint Generation;

            public struct ComponentData
            {
                public int TypeIndex;
                public int ItemIndex;
            }
        }
    }
}