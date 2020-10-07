using System;
using System.Collections.Generic;

namespace Ecs.Core
{
    public abstract class EntityQuery
    {
        // Input System State
        internal World World;

        // Query 
        internal int[] ComponentTypeIndices;

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

        private void ProcessPendingUpdates()
        {
            for (int i = 0; i < _pendingUpdateCount; i++)
            {
                ref readonly var pendingUpdate = ref _pendingEntityUpdates[i];
                ref readonly var entityData = ref World.GetEntityData(pendingUpdate.Entity);

                if (pendingUpdate.Operation == PendingEntityUpdate.OperationType.Add)
                {
                    Version lastVersion;
                    FindComponents(in entityData, out lastVersion);

                    AddEntityToQueryResults(in pendingUpdate.Entity, lastVersion);
                }
                else if (pendingUpdate.Operation == PendingEntityUpdate.OperationType.Change)
                {
                    Version lastVersion;
                    FindComponents(in entityData, out lastVersion);

                    UpdateEntityInQueryResults(in pendingUpdate.Entity, lastVersion);
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

        public Entity GetEntity(int index)
        {
            return _entityResults[index].Entity;
        }

        public int GetEntityCount()
        {
            return _entityCount;
        }

        protected virtual bool IsMatch(in World.EntityData entityData, out Version lastVersion)
        {
            return FindComponents(in entityData, out lastVersion);
        }

        private bool FindComponents(in World.EntityData entityData, out Version lastVersion)
        {
            lastVersion = default;

            for (int i = 0; i < ComponentTypeIndices.Length; i++)
            {
                if (!FindComponent(
                    in entityData,
                    ComponentTypeIndices[i],
                    out lastVersion))
                {
                    return false;
                }
            }

            return true;
        }

        private bool FindComponent(in World.EntityData entityData, int componentTypeIndex, out Version lastVersion)
        {
            lastVersion = default;

            for (int j = 0; j < entityData.ComponentCount; j++)
            {
                if (entityData.Components[j].TypeIndex == componentTypeIndex)
                {
                    var version =
                        this.World
                        .ComponentPools[componentTypeIndex]
                        .GetItemVersion(
                            entityData.Components[j]
                            .ItemIndex);

                    lastVersion =
                        version > lastVersion
                        ? version
                        : lastVersion;

                    return true;
                }
            }

            return false;
        }

        private bool AddPendingEntityUpdate(in PendingEntityUpdate pendingUpdate)
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

        internal virtual void OnAddEntity(in Entity entity, in World.EntityData entityData)
        {
            if (AddPendingEntityUpdate(
                new PendingEntityUpdate
                {
                    Entity = entity,
                    Operation = PendingEntityUpdate.OperationType.Add,
                }))
            {
                return;
            }
            
            Version lastVersion = default;

            if (IsMatch(in entityData, out lastVersion))
            {
                AddEntityToQueryResults(entity, lastVersion);
            }
        }

        internal virtual void OnChangeEntity(in Entity entity, in World.EntityData entityData)
        {
            if (AddPendingEntityUpdate(
                new PendingEntityUpdate
                {
                    Entity = entity,
                    Operation = PendingEntityUpdate.OperationType.Change,
                }))
            {
                return;
            }

            Version lastVersion = default;

            if (IsMatch(in entityData, out lastVersion))
            {
                UpdateEntityInQueryResults(entity, lastVersion);
            }
        }

        internal virtual void OnRemoveEntity(in Entity entity, in World.EntityData entityData)
        {
            if (AddPendingEntityUpdate(
                new PendingEntityUpdate
                {
                    Entity = entity,
                    Operation = PendingEntityUpdate.OperationType.Remove,
                }))
            {
                return;
            }

            Version lastVersion = default;

            if (IsMatch(in entityData, out lastVersion))
            {
                RemoveEntityFromQueryResults(entity);
            }
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

        private void UpdateEntityInQueryResults(in Entity entity, Version lastVersion)
        {
            var queryEntityIndex = _entityIndexToQueryIndex[entity.Id];

            // Update the component version.
            _entityResults[queryEntityIndex].ComponentVersion = lastVersion;
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

        public struct EntityItem
        {
            public Entity Entity;
            public Version ComponentVersion;
        }

        internal protected struct PendingEntityUpdate
        {
            public Entity Entity;
            public OperationType Operation;

            public enum OperationType
            { 
                Add,
                Change,
                Remove
            }
        }
    }

    public class EntityQuery<T> : EntityQuery where T : struct
    {
        protected EntityQuery(World world) : base(world)
        {
            this.ComponentTypeIndices = new[] { ComponentType<T>.Index };
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
    }

    public class EntityQueryWithChangeFilter<T> : EntityQuery where T : struct
    {

        protected EntityQueryWithChangeFilter(World world) : base(world)
        {
            this.ComponentTypeIndices = new[] { ComponentType<T>.Index };
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
    }
}

