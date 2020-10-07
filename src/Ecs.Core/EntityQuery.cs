using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ecs.Core
{
    public abstract class EntityQuery
    {
        // Input System State
        internal World World;

        // Query 
        internal int[] IncludedComponentTypeIndices;
        internal int[] ExcludedComponentTypeIndices;

        // Query Results
        internal protected EntityItem[] _entityResults = new EntityItem[EcsConstants.InitialEntityQueryEntityCapacity];
        internal protected int _entityCount = 0;

        private Dictionary<int, int> _entityIndexToQueryIndex = new Dictionary<int, int>(EcsConstants.InitialEntityQueryEntityCapacity);

        // Locking & Pending Changes 
        private int _lockCount = 0;

        private PendingEntityUpdate[] _pendingEntityUpdates = new PendingEntityUpdate[EcsConstants.InitialEntityQueryEntityCapacity];
        int _pendingUpdateCount = 0;

        internal protected EntityQuery(World world)
        {
            World = world;
        }

        public void Lock()
        {
            _lockCount++;
        }

        public void Unlock()
        {
#if DEBUG
            if (_lockCount == 0)
            {
                throw new InvalidOperationException($"Tried to unlock filter with no locks. Type={GetType().Name}");
            }
#endif

            _lockCount--;

            if (_lockCount == 0 && _pendingUpdateCount > 0)
            {
                ProcessPendingUpdates();
            }
        }

        public Entity GetEntity(int index)
        {
            return _entityResults[index].Entity;
        }

        public int GetEntityCount()
        {
            return _entityCount;
        }

        public bool IsEmpty()
        {
            return _entityCount == 0;
        }

        internal virtual void OnAddIncludeComponent(
            in Entity entity,
            in World.EntityData entityData,
            int componentTypeIndex)
        {
            if (_entityIndexToQueryIndex.ContainsKey(entity.Id))
            {
                return;
            }

            Version componentVersion;
            FindComponentInEntity(entityData, componentTypeIndex, out componentVersion);

            if (QueueEntityUpdate(
                new PendingEntityUpdate
                {
                    Entity = entity,
                    ComponentTypeIndex = componentTypeIndex,
                    ComponentVersion = componentVersion,
                    Operation = PendingEntityUpdate.OperationType.Add,
                }))
            {
                return;
            }

            AddEntityToQueryResults(entity, componentVersion);
        }

        internal virtual void OnAddExcludeComponent(
            in Entity entity,
            in World.EntityData entityData,
            int componentTypeIndex)
        {
            if (!_entityIndexToQueryIndex.ContainsKey(entity.Id))
            {
                return;
            }

            for (int i = 0; i < ExcludedComponentTypeIndices.Length; i++)
            {
                if (ExcludedComponentTypeIndices[i] == componentTypeIndex)
                {
                    continue;
                }

                for (int j = 0; j < entityData.ComponentCount; j++)
                {
                    if (entityData.Components[j].TypeIndex == ExcludedComponentTypeIndices[i])
                    {

                       // At least one excluded component already exists.
                        return;
                    }
                }
            }

            // Remove this entity from results. An excluded component was added.

            if (QueueEntityUpdate(
                new PendingEntityUpdate
                {
                    Entity = entity,
                    ComponentTypeIndex = componentTypeIndex,
                    Operation = PendingEntityUpdate.OperationType.Remove,
                }))
            {
                return;
            }

            RemoveEntityFromQueryResults(entity);
        }

        internal virtual void OnChangeIncludeComponent(
            in Entity entity,
            in World.EntityData entityData,
            int componentTypeIndex)
        {
#if DEBUG
            if (!_entityIndexToQueryIndex.ContainsKey(entity.Id))
            {
                throw new InvalidOperationException($"Called {nameof(OnChangeIncludeComponent)} on component not found in the include list.");
            }
#endif

            Version componentVersion;
            FindComponentInEntity(entityData, componentTypeIndex, out componentVersion);

            if (QueueEntityUpdate(
                new PendingEntityUpdate
                {
                    Entity = entity,
                    ComponentTypeIndex = componentTypeIndex,
                    ComponentVersion = componentVersion,
                    Operation = PendingEntityUpdate.OperationType.Change,
                }))
            {
                return;
            }

            DirtyEntityInQueryResults(entity, componentVersion);
        }

        internal virtual void OnRemoveIncludeComponent(
            in Entity entity,
            in World.EntityData entityData,
            int componentTypeIndex)
        {
            if (!_entityIndexToQueryIndex.ContainsKey(entity.Id))
            {
                return;
            }

            if (QueueEntityUpdate(
                new PendingEntityUpdate
                {
                    Entity = entity,
                    ComponentTypeIndex = componentTypeIndex,
                    Operation = PendingEntityUpdate.OperationType.Remove,
                }))
            {
                return;
            }

            RemoveEntityFromQueryResults(entity);
        }

        internal virtual void OnRemoveExcludeComponent(
            in Entity entity,
            in World.EntityData entityData,
            int componentTypeIndex)
        {
            Debug.Assert(!_entityIndexToQueryIndex.ContainsKey(entity.Id));

            for (int i = 0; i < ExcludedComponentTypeIndices.Length; i++)
            {
                if (ExcludedComponentTypeIndices[i] == componentTypeIndex)
                {
                    continue;
                }

                for (int j = 0; j < entityData.ComponentCount; j++)
                {
                    if (entityData.Components[j].TypeIndex == ExcludedComponentTypeIndices[i])
                    {
                        // At least one excluded component remains.
                        return;
                    }
                }
            }

            // Add this entity from results. No excluded components are attached to the entity.

            Version componentVersion;
            FindComponentInEntity(entityData, componentTypeIndex, out componentVersion);

            if (QueueEntityUpdate(
                new PendingEntityUpdate
                {
                    Entity = entity,
                    ComponentTypeIndex = componentTypeIndex,
                    ComponentVersion = componentVersion,
                    Operation = PendingEntityUpdate.OperationType.Add,
                }))
            {
                return;
            }

            AddEntityToQueryResults(entity, componentVersion);
        }

        private void AddEntityToQueryResults(in Entity entity, Version lastVersion)
        {
            if (_entityResults.Length == _entityCount)
            {
                Array.Resize(ref _entityResults, 2 * _entityCount);
            }

            _entityIndexToQueryIndex[entity.Id] = _entityCount;
            _entityResults[_entityCount++] = new EntityItem
            {
                Entity = entity,
                ComponentVersion = lastVersion,
            };
        }

        private bool FindComponentInEntity(in World.EntityData entityData, int componentTypeIndex, out Version version)
        {
            version = default;

            for (int j = 0; j < entityData.ComponentCount; j++)
            {
                if (entityData.Components[j].TypeIndex == componentTypeIndex)
                {
                    version =
                        this.World
                        .ComponentPools[componentTypeIndex]
                        .GetItemVersion(
                            entityData.Components[j]
                            .ItemIndex);

                    return true;
                }
            }

            return false;
        }

        private bool QueueEntityUpdate(in PendingEntityUpdate pendingUpdate)
        {
            if (_lockCount == 0)
            {
                return false;
            }

            if (_pendingEntityUpdates.Length == _pendingUpdateCount)
            {
                Array.Resize(ref _pendingEntityUpdates, 2 * _pendingUpdateCount);
            }

            _pendingEntityUpdates[_pendingUpdateCount++] = pendingUpdate;

            return true;
        }

        private void DirtyEntityInQueryResults(in Entity entity, Version version)
        {
            var queryEntityIndex = _entityIndexToQueryIndex[entity.Id];

            // Update the component version.
            _entityResults[queryEntityIndex].ComponentVersion =
                _entityResults[queryEntityIndex].ComponentVersion > version
                ? _entityResults[queryEntityIndex].ComponentVersion
                : version;
        }

        private void RemoveEntityFromQueryResults(in Entity entity)
        {
            var queryEntityIndex = _entityIndexToQueryIndex[entity.Id];

            // Move the last item to removed position.
            if (queryEntityIndex < _entityCount - 1)
            {
                _entityResults[queryEntityIndex] = _entityResults[_entityCount - 1];
            }

            _entityIndexToQueryIndex.Remove(entity.Id);

            _entityCount--;
        }

        private void ProcessPendingUpdates()
        {
            for (int i = 0; i < _pendingUpdateCount; i++)
            {
                ref readonly var pendingUpdate = ref _pendingEntityUpdates[i];
                ref readonly var entityData = ref World.GetEntityData(pendingUpdate.Entity);

                if (pendingUpdate.Operation == PendingEntityUpdate.OperationType.Add)
                {
                    AddEntityToQueryResults(in pendingUpdate.Entity, pendingUpdate.ComponentVersion);
                }
                else if (pendingUpdate.Operation == PendingEntityUpdate.OperationType.Change)
                {
                    DirtyEntityInQueryResults(in pendingUpdate.Entity, pendingUpdate.ComponentVersion);
                }
                else if (pendingUpdate.Operation == PendingEntityUpdate.OperationType.Remove)
                {
                    RemoveEntityFromQueryResults(pendingUpdate.Entity);
                }
#if DEBUG
                else
                {
                    throw new InvalidOperationException($"Unknown operation type: {nameof(PendingEntityUpdate.OperationType)}={pendingUpdate.Operation}");
                }
#endif
            }

            _pendingUpdateCount = 0;
        }

        internal protected struct EntityItem
        {
            public Entity Entity;
            public Version ComponentVersion;
        }

        internal protected struct PendingEntityUpdate
        {
            public Entity Entity;
            public int ComponentTypeIndex;
            public Version ComponentVersion;
            public OperationType Operation;

            public enum OperationType
            { 
                Add,
                Change,
                Remove
            }
        }
    }

    public class EntityQuery<IncType> : EntityQuery where IncType : struct
    {
        protected EntityQuery(World world) : base(world)
        {
            this.IncludedComponentTypeIndices = new[] { ComponentType<IncType>.Index };
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator : IDisposable
        {
            EntityQuery _query;
            private int _current;

            internal Enumerator(EntityQuery query)
            {
                _query = query ?? throw new ArgumentNullException(nameof(query));
                _current = -1;

                _query.Lock();
            }

            public Entity Current => _query._entityResults[_current].Entity;

            public void Dispose()
            {
                _query.Unlock();
            }

            public bool MoveNext()
            {
                return ++_current < _query._entityCount;
            }
        }

        public class Exclude<ExcType> : EntityQuery<IncType> where ExcType : struct
        {
            protected Exclude(World world) : base(world)
            {
                ExcludedComponentTypeIndices = new[]
                {
                    ComponentType<ExcType>.Index,
                };
            }
        }

        public class Exclude<ExcType1, ExcType2> : EntityQuery<IncType>
            where ExcType1 : struct
            where ExcType2 : struct
        {
            protected Exclude(World world) : base(world)
            {
                ExcludedComponentTypeIndices = new[]
                {
                    ComponentType<ExcType1>.Index,
                    ComponentType<ExcType2>.Index,
                };
            }
        }
    }

    public class EntityQueryWithChangeFilter<IncType> : EntityQuery where IncType : struct
    {
        protected EntityQueryWithChangeFilter(World world) : base(world)
        {
            this.IncludedComponentTypeIndices = new[] { ComponentType<IncType>.Index };
        }

        public ChangeFilteredEnumerator GetEnumerator()
        {
            return new ChangeFilteredEnumerator(
                this,
                World.LastSystemVersion);
        }

        public struct ChangeFilteredEnumerator : IDisposable
        {
            private EntityQuery _query;
            private int _current;

            private Version _lastSystemVersion;

            internal ChangeFilteredEnumerator(
                EntityQuery query,
                Version lastSystemVersion)
            {
                _query = query ?? throw new ArgumentNullException(nameof(query));
                _current = -1;

                _query.Lock();

                _lastSystemVersion = lastSystemVersion;
            }

            public Entity Current => _query._entityResults[_current].Entity;

            public void Dispose()
            {
                _query.Unlock();
            }

            public bool MoveNext()
            {
                while (++_current < _query._entityCount)
                {
                    if (_lastSystemVersion < _query._entityResults[_current].ComponentVersion ||
                        _lastSystemVersion == Version.Zero)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public class Exclude<ExcType> : EntityQueryWithChangeFilter<IncType> where ExcType : struct
        {
            protected Exclude(World world) : base(world)
            {
                ExcludedComponentTypeIndices = new[]
                {
                    ComponentType<ExcType>.Index,
                };
            }
        }

        public class Exclude<ExcType1, ExcType2> : EntityQueryWithChangeFilter<IncType> 
            where ExcType1: struct 
            where ExcType2 : struct
        {
            protected Exclude(World world) : base(world)
            {
                ExcludedComponentTypeIndices = new[]
                {
                    ComponentType<ExcType1>.Index,
                    ComponentType<ExcType2>.Index,
                };
            }
        }
    }
}

