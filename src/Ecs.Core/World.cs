using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Ecs.Core
{
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

        private readonly Dictionary<int, AppendOnlyList<EntityQuery>> _componentIdToEntityQueries;

        private readonly AppendOnlyList<EntityQuery> _queries;

        public World(EcsConfig config)
        {
            Config = config; 

            ComponentPools = new IComponentPool[Config.InitialComponentPoolCapacity];
            _entities = new EntityData[Config.InitialEntityPoolCapacity];
            _freeEntityIds = new int[Config.InitialEntityPoolCapacity];

            _componentIdToEntityQueries = new Dictionary<int, AppendOnlyList<EntityQuery>>(Config.InitialComponentToEntityQueryMapCapacity);
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

            for (int i = 0; i < entityQuery.ComponentTypeIndices.Length; i++)
            {
                if (!_componentIdToEntityQueries.ContainsKey(entityQuery.ComponentTypeIndices[i]))
                {
                    _componentIdToEntityQueries[entityQuery.ComponentTypeIndices[i]] = new AppendOnlyList<EntityQuery>(EcsConstants.InitialEntityQueryEntityCapacity);
                }

                _componentIdToEntityQueries[entityQuery.ComponentTypeIndices[i]].Add(entityQuery);
            }

            return entityQuery;
        }

        public void OnAddEntity(
            int componentTypeIndex, 
            in Entity entity, 
            in EntityData entityData)
        {
            AppendOnlyList<EntityQuery> entityQueries;

            if (_componentIdToEntityQueries.TryGetValue(componentTypeIndex, out entityQueries))
            {
                for (int i = 0; i < entityQueries.Count; i++)
                {
                    var entityQuery = entityQueries.Items[i];

                    entityQuery.OnAddEntity(entity, entityData);
                }
            }
        }

        public void OnChangeEntity(
            int componentTypeIndex,
            in Entity entity,
            in EntityData entityData)
        {
            AppendOnlyList<EntityQuery> entityQueries;

            if (_componentIdToEntityQueries.TryGetValue(componentTypeIndex, out entityQueries))
            {
                for (int i = 0; i < entityQueries.Count; i++)
                {
                    var entityQuery = entityQueries.Items[i];

                    entityQuery.OnChangeEntity(entity, entityData);
                }
            }
        }

        public void OnRemoveEntity(
            int componentTypeIndex,
            in Entity entity,
            in EntityData entityData)
        {
            AppendOnlyList<EntityQuery> entityQueries;

            if (_componentIdToEntityQueries.TryGetValue(componentTypeIndex, out entityQueries))
            {
                for (int i = 0; i < entityQueries.Count; i++)
                {
                    var entityQuery = entityQueries.Items[i];

                    entityQuery.OnRemoveEntity(entity, entityData);
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
                public Version Version;
            }
        }
    }
}