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
        public struct WorldState
        {
            internal Version GlobalSystemVersion;
            internal Version LastSystemVersion;

            internal IComponentPool[] ComponentPools;

            internal World.EntityData[] _entities;
            internal int[] _freeEntityIds;

            internal int _entityCount;
            internal int _freeEntityCount;

            internal AppendOnlyList<EntityQuery> _queries;
        }

        public static WorldState CopyState(in WorldState state)
        {
            var copiedState = new WorldState
            {
                GlobalSystemVersion = state.GlobalSystemVersion,
                LastSystemVersion = state.LastSystemVersion,

                _freeEntityIds = state._freeEntityIds,
                _entityCount = state._entityCount,
                _freeEntityCount = state._freeEntityCount,

            };

            copiedState.ComponentPools = new IComponentPool[state.ComponentPools.Length];
            Array.Copy(state.ComponentPools, copiedState.ComponentPools, state.ComponentPools.Length);
 
            copiedState._entities = new EntityData[state._entities.Length];
            Array.Copy(state._entities, copiedState._entities, state._entityCount);

            copiedState._freeEntityIds = new int[state._freeEntityIds.Length];
            Array.Copy(state._freeEntityIds, copiedState._freeEntityIds, state._freeEntityCount);

            copiedState._queries = new AppendOnlyList<EntityQuery>(state._queries.Count);
            for (int i = 0; i < state._queries.Count; i++)
            {
                copiedState._queries.Items[i] = state._queries.Items[i];
            }

            return copiedState;
        }

        internal readonly EcsConfig Config;

        public WorldState State;

        private readonly Dictionary<int, AppendOnlyList<EntityQuery>> _includedComponentIdToEntityQueries;
        private readonly Dictionary<int, AppendOnlyList<EntityQuery>> _excludedComponentIdToEntityQueries;



        public World(EcsConfig config)
        {
            Config = config;

            State.ComponentPools = new IComponentPool[Config.InitialComponentPoolCapacity];
            State._entities = new EntityData[Config.InitialEntityPoolCapacity];
            State._freeEntityIds = new int[Config.InitialEntityPoolCapacity];

            _includedComponentIdToEntityQueries = new Dictionary<int, AppendOnlyList<EntityQuery>>(Config.InitialComponentToEntityQueryMapCapacity);
            _excludedComponentIdToEntityQueries = new Dictionary<int, AppendOnlyList<EntityQuery>>(Config.InitialComponentToEntityQueryMapCapacity);
            State._queries = new AppendOnlyList<EntityQuery>(Config.InitialEntityQueryCapacity);

            State.GlobalSystemVersion = new Version();
            State.LastSystemVersion = State.GlobalSystemVersion;
        }

        public Entity NewEntity()
        {
            var entity = new Entity
            {
                World = this
            };

            if (State._freeEntityCount > 0)
            {
                entity.Id = State._freeEntityIds[--State._freeEntityCount];

                ref var entityData = ref State._entities[entity.Id];
                entity.Generation = entityData.Generation;
                entityData.ComponentCount = 0;
            }
            else
            {
                entity.Id = GetNextEntityId();

                ref var entityData = ref State._entities[entity.Id];

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
            State._freeEntityIds[State._freeEntityCount++] = id;
        }

        public ref EntityData GetEntityData(in Entity entity)
        {
            return ref State._entities[entity.Id];
        }

        public bool IsFreed(in Entity entity)
        {
            ref var entityData = ref State._entities[entity.Id];

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
            for (int i = 0; i < State._queries.Count; i++)
            {
                if (State._queries.Items[i].GetType() == entityQueryType)
                {
                    // Matching query exists.
                    return State._queries.Items[i];
                }
            }

            // Create query.
            var entityQuery = (EntityQuery) Activator.CreateInstance(
                entityQueryType, 
                BindingFlags.NonPublic | BindingFlags.Instance, 
                null,
                new[] { this }, 
                CultureInfo.InvariantCulture);

            State._queries.Add(entityQuery);

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

            if (State.ComponentPools.Length < poolIndex)
            {
                var len = State.ComponentPools.Length * 2;

                while (len <= poolIndex)
                {
                    len *= 2;
                }

                Array.Resize(ref State.ComponentPools, len);
            }

            var pool = (ComponentPool<T>)State.ComponentPools[poolIndex];

            if (pool == null)
            {
                pool = new ComponentPool<T>();
                State.ComponentPools[poolIndex] = pool;
            }

            return pool;
        }

        private int GetNextEntityId()
        {
            if (State._freeEntityCount > 0)
            {
                return State._freeEntityIds[State._freeEntityCount--];
            }

            if (State._entityCount == State._entities.Length)
            {
                Array.Resize(ref State._entities, State._entityCount * 2);
            }

            return State._entityCount++;
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