using Ecs.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ecs.Core
{
    public abstract class EntityQueryBase
    {
        // Input System State
        internal World World;

        // Query 
        internal int[] IncludedComponentTypeIndices;
        internal int[] ExcludedComponentTypeIndices;

        // Query Results
        internal protected EntityItem[] _entityResults = new EntityItem[EcsConstants.InitialEntityQueryEntityCapacity];
        internal protected int _entityCount = 0;

        protected Dictionary<int, int> _entityIndexToQueryIndex = new Dictionary<int, int>(EcsConstants.InitialEntityQueryEntityCapacity);

        internal protected int[] _componentIds = null;

        // Locking & Pending Changes 
        protected int _lockCount = 0;

        protected PendingEntityUpdate[] _pendingEntityUpdates = new PendingEntityUpdate[EcsConstants.InitialEntityQueryEntityCapacity];
        protected int _pendingUpdateCount = 0;

        internal protected EntityQueryBase(World world)
        {
            World = world;
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

        internal abstract EntityQueryBase Clone();

        internal virtual void CopyTo(EntityQueryBase destQuery)
        {
            // REFERENCE
            destQuery.World = this.World;

            this.IncludedComponentTypeIndices.CopyToResize(destQuery.IncludedComponentTypeIndices);
            if (this.ExcludedComponentTypeIndices == null)
            {
                destQuery.ExcludedComponentTypeIndices = null;
            }
            else
            {
                if (destQuery.ExcludedComponentTypeIndices == null)
                {
                    destQuery.ExcludedComponentTypeIndices = new int[this.ExcludedComponentTypeIndices.Length];
                }
                
                this.ExcludedComponentTypeIndices.CopyToResize(destQuery.ExcludedComponentTypeIndices);
            }

            if (destQuery._entityResults.Length < this._entityResults.Length)
            {
                Array.Resize(ref destQuery._entityResults, this._entityResults.Length);
            }

            for (int i = 0; i < this._entityResults.Length; i++)
            {
                // Copy since only reference is World and it never changes.
                this._entityResults[i].CopyTo(ref destQuery._entityResults[i]);
            }

            destQuery._entityCount = this._entityCount;

            // ### HEAP ALLOCATION
            destQuery._entityIndexToQueryIndex = new Dictionary<int, int>(this._entityIndexToQueryIndex);

            this._componentIds.CopyToResize(destQuery._componentIds);

            destQuery._lockCount = this._lockCount;

            if (destQuery._pendingEntityUpdates.Length < this._pendingEntityUpdates.Length)
            {
                Array.Resize(ref destQuery._pendingEntityUpdates, this._pendingEntityUpdates.Length);
            }

            for (int i = 0; i < this._pendingEntityUpdates.Length; i++)
            {
                this._pendingEntityUpdates[i].CopyTo(ref destQuery._pendingEntityUpdates[i]);
            }

            destQuery._pendingUpdateCount = this._pendingUpdateCount;
        }

        internal void Lock()
        {
            _lockCount++;
        }

        internal void Unlock()
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
            int componentIndex;
            FindComponentInEntity(entityData, componentTypeIndex, out componentVersion, out componentIndex);

            if (QueueEntityUpdate(
                new PendingEntityUpdate
                {
                    Entity = entity,
                    ComponentIndex = componentIndex,
                    ComponentTypeIndex = componentTypeIndex,
                    ComponentVersion = componentVersion,
                    Operation = PendingEntityUpdate.OperationType.Add,
                }))
            {
                return;
            }

            AddEntityToQueryResults(entity, componentVersion, componentIndex);
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
            int componentIndex;
            FindComponentInEntity(entityData, componentTypeIndex, out componentVersion, out componentIndex);

            if (QueueEntityUpdate(
                new PendingEntityUpdate
                {
                    Entity = entity,
                    ComponentIndex = componentIndex,
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
            int componentIndex;
            FindComponentInEntity(entityData, componentTypeIndex, out componentVersion, out componentIndex);

            if (QueueEntityUpdate(
                new PendingEntityUpdate
                {
                    Entity = entity,
                    ComponentIndex = componentIndex,
                    ComponentTypeIndex = componentTypeIndex,
                    ComponentVersion = componentVersion,
                    Operation = PendingEntityUpdate.OperationType.Add,
                }))
            {
                return;
            }

            AddEntityToQueryResults(entity, componentVersion, componentIndex);
        }

        private bool FindComponentInEntity(in World.EntityData entityData, int componentTypeIndex, out Version version, out int componentIndex)
        {
            version = default;

            for (componentIndex = 0; componentIndex < entityData.ComponentCount; componentIndex++)
            {
                if (entityData.Components[componentIndex].TypeIndex == componentTypeIndex)
                {
                    version =
                        this.World
                        .State.ComponentPools[componentTypeIndex]
                        .GetItemVersion(
                            entityData.Components[componentIndex]
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
            _entityResults[queryEntityIndex].LatestComponentVersion =
                _entityResults[queryEntityIndex].LatestComponentVersion > version
                ? _entityResults[queryEntityIndex].LatestComponentVersion
                : version;
        }

        private void AddEntityToQueryResults(in Entity entity, Version lastVersion, int componentIndex)
        {
            if (_entityResults.Length == _entityCount)
            {
                Array.Resize(ref _entityResults, 2 * _entityCount);

                if (_componentIds != null)
                {
                    Array.Resize(ref _componentIds, 2 * _entityCount);
                }
            }

            _entityIndexToQueryIndex[entity.Id] = _entityCount;
            _entityResults[_entityCount] = new EntityItem
            {
                Entity = entity,
                LatestComponentVersion = lastVersion,
            };

            if (_componentIds != null)
            {
                _componentIds[_entityCount] = componentIndex;
            }

            _entityCount++;
        }

        private void RemoveEntityFromQueryResults(in Entity entity)
        {
            var queryEntityIndex = _entityIndexToQueryIndex[entity.Id];

            // Move the last item to removed position.
            if (queryEntityIndex < _entityCount - 1)
            {
                _entityResults[queryEntityIndex] = _entityResults[_entityCount - 1];

                if (_componentIds != null)
                {
                    _componentIds[queryEntityIndex] = _componentIds[_entityCount - 1];
                }
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
                    AddEntityToQueryResults(in pendingUpdate.Entity, pendingUpdate.ComponentVersion, pendingUpdate.ComponentIndex);
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
            public Version LatestComponentVersion;
        }

        internal protected struct PendingEntityUpdate
        {
            public Entity Entity;
            public int ComponentTypeIndex;
            public int ComponentIndex;
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

    /// <summary>
    /// Base class for globally shared (per world) entity queries.
    /// </summary>
    public abstract class GlobalEntityQuery : EntityQueryBase
    {
        public GlobalEntityQuery(World world) 
            : base(world)
        {
        }
    }

    public class EntityQuery<IncType> : GlobalEntityQuery
        where IncType : unmanaged
    {
        private ComponentPool<IncType> _componentPool;

        internal EntityQuery(World world) 
            : base(world)
        {
            this.IncludedComponentTypeIndices = new[] { ComponentType<IncType>.Index };

            _componentPool = world.GetPool<IncType>();
            _componentIds = new int[EcsConstants.InitialEntityQueryEntityCapacity];
        }

        public ref readonly IncType GetReadonly(int index)
        {
            return ref _componentPool.Items[_componentIds[index]].Item;
        }

        public ComponentEnumerable<IncType> GetComponents() 
        {
            return new ComponentEnumerable<IncType>(this, _componentIds);
        }

        public EntityEnumerator GetEnumerator()
        {
            return new EntityEnumerator(this);
        }

        public struct ComponentEnumerable<T>
            where T : unmanaged
        {
            private readonly EntityQueryBase _query;
            private readonly int[] _componentIds;

            public ComponentEnumerable(EntityQueryBase query, int[] componentIds)
            {
                _query = query;
                _componentIds = componentIds;
            }

            public ComponentEnumerator<T> GetEnumerator()
            {
                return new ComponentEnumerator<T>(_query, _componentIds);
            }
        }

        public struct ComponentEnumerator<T> : IDisposable
            where T : unmanaged
        {
            private readonly EntityQueryBase _query;
            private readonly ComponentPool<T> _componentPool;
            private readonly int[] _componentIds;
            private readonly Version _globalSystemVersion;

            private int _current;

            internal ComponentEnumerator(EntityQueryBase query, int[] componentIds)
            {
                _query = query ?? throw new ArgumentNullException(nameof(query));
                _componentPool = _query.World.GetPool<T>();
                _componentIds = componentIds;
                _globalSystemVersion = query.World.State.GlobalSystemVersion;

                _current = -1;

                _query.Lock();
            }

            public ref T Current
            {
                get
                { 
                    ref var item = ref _componentPool.Items[_componentIds[_current]];

                    item.Version = _globalSystemVersion;

                    return ref item.Item;
                }
            }

            public void Dispose()
            {
                _query.Unlock();
            }

            public bool MoveNext()
            {
                return ++_current < _query._entityCount;
            }
        }

        internal override EntityQueryBase Clone()
        {
            var query = new EntityQuery<IncType>(this.World);

            CopyTo(query);

            return query;
        }

        internal override void CopyTo(EntityQueryBase queryBase)
        {
            base.CopyTo(queryBase);

            var entityQuery = (EntityQuery<IncType>)queryBase;

            entityQuery._componentPool = this._componentPool;
        }

        public struct EntityEnumerator : IDisposable
        {
            EntityQueryBase _query;
            private int _current;

            internal EntityEnumerator(EntityQueryBase query)
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

        public class Exclude<ExcType> : EntityQuery<IncType> 
            where ExcType : unmanaged
        {
            internal Exclude(World world) : base(world)
            {
                ExcludedComponentTypeIndices = new[]
                {
                    ComponentType<ExcType>.Index,
                };
            }
        }

        public class Exclude<ExcType1, ExcType2> : EntityQuery<IncType>
            where ExcType1 : unmanaged
            where ExcType2 : unmanaged
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

    /// <summary>
    /// Base class for unsharable entity queries.
    /// </summary>
    public abstract class PerSystemsEntityQuery : EntityQueryBase
    {
        internal protected int _systemsIndex;

        public PerSystemsEntityQuery(World world, int systemsIndex) :
            base(world)
        {
            _systemsIndex = systemsIndex;
        }
    }

    public class EntityQueryWithChangeFilter<IncType> : PerSystemsEntityQuery 
        where IncType : unmanaged
    {
        private ComponentPool<IncType> _componentPool;

        internal EntityQueryWithChangeFilter(World world, int systemsIndex) 
            : base(world, systemsIndex)
        {
            this.IncludedComponentTypeIndices = new[] { ComponentType<IncType>.Index };

            _componentPool = world.GetPool<IncType>();
            _componentIds = new int[EcsConstants.InitialEntityQueryEntityCapacity];
        }

        public ref readonly IncType GetReadonly(int index)
        {
            return ref _componentPool.Items[_componentIds[index]].Item;
        }

        public ref IncType Get(int index, Version version)
        {
            ref var item = ref _componentPool.Items[_componentIds[index]];

            item.Version = version;

            return ref item.Item;
        }
#if MOTHBALL
        public IncType[] GetComponentArray(Version lastSystemVersion)
        {
            AppendOnlyList<IncType> list = new AppendOnlyList<IncType>(64);

            for (int i = 0; i < _entityCount; i++)
            {
                if (lastSystemVersion < _componentPool.Items[_componentIds[i]].Version)
                {
                    list.Add(_componentPool.Items[_componentIds[i]].Item);
                }
            }

            return list.Items;
        }
#endif //MOTHBALL

        public ChangeFilteredEnumerator GetEnumerator()
        {
            return new ChangeFilteredEnumerator(
                this,
                World.State.LastSystemVersion.Items[_systemsIndex]);
        }

        internal override EntityQueryBase Clone()
        {
            var query = new EntityQueryWithChangeFilter<IncType>(this.World, this._systemsIndex);

            CopyTo(query);

            return query;
        }

        internal override void CopyTo(EntityQueryBase queryBase)
        {
            base.CopyTo(queryBase);

            var entityQuery = (EntityQueryWithChangeFilter<IncType>)queryBase;

            // PerSystemsEntityQuery
            entityQuery._systemsIndex = this._systemsIndex;

            // EntityQueryWithChangeFilter
            entityQuery._componentPool = this._componentPool;
        }

        public struct ChangeFilteredEnumerator : IDisposable
        {
            private EntityQueryBase _query;
            private int _current;

            private Version _lastSystemVersion;

            internal ChangeFilteredEnumerator(
                EntityQueryBase query,
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
                    if (_lastSystemVersion < _query._entityResults[_current].LatestComponentVersion ||
                        _lastSystemVersion == Version.Zero)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public class Exclude<ExcType> : EntityQueryWithChangeFilter<IncType> 
            where ExcType : unmanaged
        {
            internal Exclude(World world, int systemsIndex)
                : base(world, systemsIndex)
            {
                ExcludedComponentTypeIndices = new[]
                {
                    ComponentType<ExcType>.Index,
                };
            }
        }

        public class Exclude<ExcType1, ExcType2> : EntityQueryWithChangeFilter<IncType>
            where ExcType1 : unmanaged
            where ExcType2 : unmanaged
        {
            protected Exclude(World world, int systemsIndex)
                : base(world, systemsIndex)
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

