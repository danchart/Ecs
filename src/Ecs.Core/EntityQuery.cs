using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ecs.Core
{

    public abstract class EntityQuery
    {
        // Input System State
        internal World World;

        // Query 
        internal int[] ComponentTypeIndices;

        // Query Results
        internal protected EntityResultData[] _entityResults = new EntityResultData[EcsConstants.InitialEntityQueryEntityCapacity];
        internal protected int _entityCount = 0;

        private Dictionary<int, int> _entityIndexToQueryIndex = new Dictionary<int, int>(EcsConstants.InitialEntityQueryEntityCapacity);

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
                    OnAddEntity(in pendingUpdate.Entity, entityData);
                }
                else if (pendingUpdate.Operation == PendingEntityUpdate.OperationType.Change)
                {
                    OnChangeEntity(in pendingUpdate.Entity, entityData);
                }
                else if (pendingUpdate.Operation == PendingEntityUpdate.OperationType.Remove)
                {
                    OnRemoveEntity(in pendingUpdate.Entity, entityData);
                }
#if DEBUG
                else
                {
                    throw new InvalidOperationException($"Unknown operation type: {nameof(PendingEntityUpdate.OperationType)}={pendingUpdate.Operation}");
                }
#endif
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

        protected virtual bool IsMatch(in World.EntityData entityData, out Version lastVersion)
        {
            lastVersion = default;

            for (int i = 0; i < ComponentTypeIndices.Length; i++)
            {
                bool hasComponent = false;

                for (int j = 0; j < entityData.ComponentCount; j++)
                {
                    if (entityData.Components[j].TypeIndex == ComponentTypeIndices[i])
                    {
                        hasComponent = true;

                        lastVersion =
                            entityData.Components[j].Version > lastVersion
                            ? entityData.Components[j].Version
                            : lastVersion;

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
                if (_entityResults.Length == _entityCount)
                {
                    Array.Resize(ref _entityResults, 2 * _entityCount);
                }

                _entityIndexToQueryIndex[entity.Id] = _entityCount;
                _entityResults[_entityCount++] = new EntityResultData
                {
                    Entity = entity,
                    ComponentVersion = lastVersion,
                };
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
                var queryEntityIndex = _entityIndexToQueryIndex[entity.Id];

                _entityResults[queryEntityIndex].ComponentVersion = lastVersion;
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
                var queryEntityIndex = _entityIndexToQueryIndex[entity.Id];

                // Move the last item to removed position.
                if (queryEntityIndex < _entityCount - 1)
                {
                    _entityResults[queryEntityIndex] = _entityResults[_entityCount - 1];
                }

                _entityIndexToQueryIndex.Remove(entity.Id);

                _entityCount--;
            }
        }

        public virtual Enumerator GetEnumerator()
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

        public struct EntityResultData
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
    }

    public class EntityQueryWithChangeFilter<T> : EntityQuery<T> where T : struct
    {
        protected EntityQueryWithChangeFilter(World world) : base(world)
        {
        }

        public virtual ChangeFilteredEnumerator GetEnumerator()
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

#if MOTHBALL
    public abstract class EntityQuery
    {
        internal int[] ComponentTypeIndices;

        protected Entity[] _entities = new Entity[EcsConstants.InitialEntityQueryEntityCapacity];
        protected int _entityCount = 0;

        private Dictionary<int, int> _entityIndexToQueryIndex = new Dictionary<int, int>(EcsConstants.InitialEntityQueryEntityCapacity);

        protected World _world;

        protected EntityQuery(World world)
        {
            _world = world;
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
        public virtual bool IsMatch(in World.EntityData entityData)
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

        internal virtual void OnAddEntity(in Entity entity, in World.EntityData entityData)
        {
            if (_entities.Length == _entityCount)
            {
                Array.Resize(ref _entities, 2 * _entityCount);
            }

            _entityIndexToQueryIndex[entity.Id] = _entityCount;
            _entities[_entityCount++] = entity;
        }

        internal virtual void OnRemoveEntity(in Entity entity, in World.EntityData entityData)
        {
            var queryEntityIndex = _entityIndexToQueryIndex[entity.Id];

            // Move the last item to removed position.
            if (queryEntityIndex < _entityCount - 1)
            {
                _entities[queryEntityIndex] = _entities[_entityCount - 1];
            }

            _entityIndexToQueryIndex.Remove(entity.Id);

            _entityCount--;
        }

        public virtual Enumerator GetEnumerator()
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

    public class EntityQueryWithChangeFilter<T> : EntityQuery<T> where T : struct
    {
        protected EntityQueryWithChangeFilter(World world) : base(world)
        {
        }

        public virtual Enumerator GetEnumerator()
        {
            return new Enumerator(_entities, _entityCount);
        }

        public struct Enumerator
        {
            private readonly Entity[] _entities;
            private readonly int _count;
            private int _current;

            private int _componentTypeIndex;
            private Version _lastSystemVersion

            internal Enumerator(Entity[] entities, int count, int componentTypeIndex, Version lastSystemVersion)
            {
                _entities = entities ?? throw new ArgumentNullException(nameof(entities)); ;
                _count = count;
                _current = -1;

                _componentTypeIndex = componentTypeIndex;
                _lastSystemVersion = lastSystemVersion;
            }

            public Entity Current => _entities[_current];

            public bool MoveNext()
            {
                while (++_current < _count)
                {

                }

                return false;
            }
        }

        internal override void OnAddEntity(in Entity entity, in World.EntityData entityData)
        {
            var componentTypeIndex = ComponentType<T>.Index;

            for (int i = 0; i < entityData.ComponentCount; i++)
            {
                if (entityData.Components[i].TypeIndex == componentTypeIndex)
                {
                    if (_world.LastSystemVersion < entityData.Components[i].Version)
                    {
                        // Component changed
                        base.OnRemoveEntity(in entity, in entityData);
                    }
                    else
                    {
                        base.OnAddEntity(in entity, in entityData);
                    }
                }
            }
        }
    }
#endif //MOTHBALL
}

