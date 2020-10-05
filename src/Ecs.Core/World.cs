﻿using System;
using System.Collections.Generic;

namespace Ecs.Core
{
    public class World
    {
        public readonly EcsConfig Config;

        public IComponentPool[] ComponentPools;

        private EntityData[] _entities;
        private int[] _freeEntityIds;

        private int _entityCount = 0;
        private int _freeEntityCount = 0;

        private readonly Dictionary<int, AppendOnlyList<EntityQuery>> _componentIdToEntityQueries;

        public World(EcsConfig config)
        {
            Config = config;

            ComponentPools = new IComponentPool[Config.InitialComponentPoolCapacity];
            _entities = new EntityData[Config.InitialEntityPoolCapacity];
            _freeEntityIds = new int[Config.InitialEntityPoolCapacity];

            _componentIdToEntityQueries = new Dictionary<int, AppendOnlyList<EntityQuery>>(Config.InitialComponentToEntityQueryMapCapacity);
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
                entity.Version = entityData.Version;
                entityData.ComponentCount = 0;
            }
            else
            {
                entity.Id = GetNextEntityId();

                ref var entityData = ref _entities[entity.Id];

                entityData.ComponentCount = 0;
                entityData.Components = new EntityData.ComponentData[Config.InitialEntityComponentCapacity];
                entityData.Version = EcsConstants.InitialEntityVersion;
                entity.Version = entityData.Version;
            }

            return entity;
        }

        public void FreeEntityData(int id, ref EntityData entityData)
        {
            entityData.ComponentCount = 0;

            if (++entityData.Version == 0)
            {
                entityData.Version = EcsConstants.InitialEntityVersion;
            }

            _freeEntityIds[_freeEntityCount++] = id;
        }

        public ref EntityData GetEntityData(Entity entity)
        {
            return ref _entities[entity.Id];
        }

        public ref EntityData GetAndValidateEntityData(Entity entity)
        {
            ref var entityData = ref GetEntityData(entity);

            if (entityData.Version != entity.Version)
            {
                throw new InvalidOperationException("Accessing a destroyed entity.");
            }

            return ref entityData;
        }

        public void AddEntityQuery(EntityQuery entityQuery)
        {
            for (int i = 0; i < entityQuery.ComponentTypeIndices.Length; i++)
            {
                if (!_componentIdToEntityQueries.ContainsKey(entityQuery.ComponentTypeIndices[i]))
                {
                    _componentIdToEntityQueries[entityQuery.ComponentTypeIndices[i]] = new AppendOnlyList<EntityQuery>(EcsConstants.InitialEntityQueryEntityCapacity);
                }

                _componentIdToEntityQueries[entityQuery.ComponentTypeIndices[i]].Add(entityQuery);
            }
        }

        public void UpdateEntityQueries(
            int componentPoolIndex, 
            in Entity entity, 
            in EntityData entityData, 
            bool isDelete)
        {
            if (isDelete)
            {
                AppendOnlyList<EntityQuery> entityQueries;

                if (_componentIdToEntityQueries.TryGetValue(componentPoolIndex, out entityQueries))
                {
                    for (int i = 0; i < entityQueries.Count; i++)
                    {
                        var entityQuery = entityQueries.Items[i];

                        entityQuery.RemoveEntity(entity);
                    }
                }
            }
            else
            {
                AppendOnlyList<EntityQuery> entityQueries;

                if (_componentIdToEntityQueries.TryGetValue(componentPoolIndex, out entityQueries))
                {
                    for (int i = 0; i < entityQueries.Count; i++)
                    {
                        var entityQuery = entityQueries.Items[i];

                        entityQuery.AddEntity(entity);
                    }
                }
            }
        }

        public ComponentPool<T> GetPool<T>() where T : struct
        {
            var poolIndex = ComponentType<T>.ComponentPoolIndex;

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
            public uint Version;

            public struct ComponentData
            {
                public int PoolIndex;
                public int ItemIndex;
            }
        }
    }
}