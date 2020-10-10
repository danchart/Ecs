using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Ecs.Core
{
    /// <summary>
    /// Contains all ECS state.
    /// </summary>
    public class World
    {
        internal readonly EcsConfig Config;

        public WorldState State;

        private readonly Dictionary<int, AppendOnlyList<EntityQueryBase>> _includedComponentIdToEntityQueries;
        private readonly Dictionary<int, AppendOnlyList<EntityQueryBase>> _excludedComponentIdToEntityQueries;

        public World(EcsConfig config)
        {
            Config = config;

            State.ComponentPools = new IComponentPool[Config.InitialComponentPools];
            State._entities = new EntityData[Config.InitialEntityPoolCapacity];
            State._freeEntityIds = new int[Config.InitialEntityPoolCapacity];

            _includedComponentIdToEntityQueries = new Dictionary<int, AppendOnlyList<EntityQueryBase>>(Config.InitialComponentToEntityQueryMapCapacity);
            _excludedComponentIdToEntityQueries = new Dictionary<int, AppendOnlyList<EntityQueryBase>>(Config.InitialComponentToEntityQueryMapCapacity);
            State._sharedQueries = new AppendOnlyList<SharedEntityQuery>(Config.InitialEntityQueryCapacity);
            State._perSystemsQueries = new AppendOnlyList<AppendOnlyList<PerSystemsEntityQuery>>(Config.InitialSystemsCapacity);

            State.GlobalSystemVersion = new Version();
            State.LastSystemVersion = new AppendOnlyList<Version>(Config.InitialSystemsCapacity); 
        }

        public int NewSystems()
        {
            this.State.LastSystemVersion.Add(this.State.GlobalSystemVersion);
            this.State._perSystemsQueries.Add(new AppendOnlyList<PerSystemsEntityQuery>(Config.InitialEntityQueryCapacity));

            return this.State.LastSystemVersion.Count - 1;
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

        public bool IsFreed(in Entity entity)
        {
            ref var entityData = ref State._entities[entity.Id];

            return
                entityData.Generation != entity.Generation ||
                entityData.ComponentCount == 0;
        }

        public EntityQueryBase GetEntityQuery<T>()
        {
            return GetSharedEntityQuery(typeof(T));
        }

        internal void FreeEntityData(int id, ref EntityData entityData)
        {
            entityData.ComponentCount = 0;
            entityData.Generation++;
            State._freeEntityIds[State._freeEntityCount++] = id;
        }

        internal ref EntityData GetEntityData(in Entity entity)
        {
            return ref State._entities[entity.Id];
        }

        internal EntityQueryBase GetPerSystemsEntityQuery(Type entityQueryType, int systemsIndex)
        {
            ref var queries = ref State._perSystemsQueries.Items[systemsIndex];

            for (int i = 0; i < queries.Count; i++)
            {
                if (queries.Items[i].GetType() == entityQueryType)
                {
                    // Matching query exists.
                    return queries.Items[i];
                }
            }

            // Create query.
            var entityQuery = (PerSystemsEntityQuery)Activator.CreateInstance(
                entityQueryType,
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new object[] { this, systemsIndex }, // args: World, SystemsIndex
                CultureInfo.InvariantCulture);

            queries.Add(entityQuery);

            AddQueryToComponentIdMaps(entityQuery);

            Invariants.ValidateEntityQuery(queries, entityQueryType);

            return entityQuery;
        }

        /// <summary>
        /// Returns the shared entity query of the matching type.
        /// </summary>
        internal EntityQueryBase GetSharedEntityQuery(Type entityQueryType)
        {
            for (int i = 0; i < State._sharedQueries.Count; i++)
            {
                if (State._sharedQueries.Items[i].GetType() == entityQueryType)
                {
                    // Matching query exists.
                    return State._sharedQueries.Items[i];
                }
            }

            // Create query.
            var entityQuery = (SharedEntityQuery)Activator.CreateInstance(
                entityQueryType,
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { this },
                CultureInfo.InvariantCulture);

            State._sharedQueries.Add(entityQuery);

            AddQueryToComponentIdMaps(entityQuery);

            Invariants.ValidateEntityQuery(State._sharedQueries, entityQueryType);

            return entityQuery;
        }

        private void AddQueryToComponentIdMaps(EntityQueryBase entityQuery)
        {
            // Add to included component->query list
            for (int i = 0; i < entityQuery.IncludedComponentTypeIndices.Length; i++)
            {
                if (!_includedComponentIdToEntityQueries.ContainsKey(entityQuery.IncludedComponentTypeIndices[i]))
                {
                    _includedComponentIdToEntityQueries[entityQuery.IncludedComponentTypeIndices[i]] = new AppendOnlyList<EntityQueryBase>(EcsConstants.InitialEntityQueryEntityCapacity);
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
                        _excludedComponentIdToEntityQueries[entityQuery.ExcludedComponentTypeIndices[i]] = new AppendOnlyList<EntityQueryBase>(EcsConstants.InitialEntityQueryEntityCapacity);
                    }

                    _excludedComponentIdToEntityQueries[entityQuery.ExcludedComponentTypeIndices[i]].Add(entityQuery);
                }
            }
        }

        internal void OnAddComponent(
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

        internal void OnChangeComponent(
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

        internal void OnRemoveComponent(
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

        internal ComponentPool<T> GetPool<T>() 
            where T : unmanaged
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
                pool = new ComponentPool<T>(Config.InitialComponentPoolCapacity);
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

        private static class Invariants
        {
            [Conditional("DEBUG")]
            public static void ValidateEntityQuery<QueryType>(AppendOnlyList<QueryType> queries, Type injectedQueryType)
            {
                var queryManifest = new HashSet<Type>();
                for (int i = 0; i < queries.Count; i++)
                {
                    var itemType = queries.Items[i].GetType();

                    if (queryManifest.Contains(itemType))
                    {
                        throw new Exception($"InvarianceViolation: Multiple copies of same query type detected. ConflictingType={itemType}, InjectedQueryType={injectedQueryType}");
                    }

                    queryManifest.Add(itemType);
                }
            }
        }
    }

    internal static class EntityDataExtensions
    {
        public static void CopyTo(in this World.EntityData srcData, ref World.EntityData dstData)
        {
            dstData.ComponentCount = srcData.ComponentCount;
            dstData.Generation = srcData.Generation;

            if (dstData.Components.Length < srcData.Components.Length)
            {
                Array.Resize(ref dstData.Components, srcData.Components.Length);
            }

            Array.Copy(srcData.Components, dstData.Components, srcData.Components.Length);
        }
    }
}