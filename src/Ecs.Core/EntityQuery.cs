using Ecs.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ecs.Core
{
    public delegate void OnEntityAddedCallback(in Entity entity);
    public delegate void OnEntityRemovedCallback(in Entity entity);

    public abstract class EntityQueryBase
    {
        // Input System State
        internal World World;

        // Listeners
        internal OnEntityAddedCallback[] _onEntityAddedCallbacks = new OnEntityAddedCallback[4];
        internal int _onEntityAddedCallbackCount = 0;

        internal OnEntityRemovedCallback[] _onEntityRemovedCallbacks = new OnEntityRemovedCallback[4];
        internal int _onEntityRemovedCallbackCount = 0;

        // Query 
        internal int[] IncludedComponentTypeIndices;
        internal int[] ExcludedComponentTypeIndices;

        // Query Results
        internal protected Entity[] _entities = new Entity[EcsConstants.InitialEntityQueryEntityCapacity];
        internal protected int _entityCount = 0;

        protected Dictionary<int, int> _entityIndexToQueryIndex = new Dictionary<int, int>(EcsConstants.InitialEntityQueryEntityCapacity);

        internal protected int[] _componentIds1 = null;
        internal protected int[] _componentIds2 = null;

        // Locking & Pending Changes 
        protected int _lockCount = 0;

        protected PendingEntityUpdate[] _pendingEntityUpdates = new PendingEntityUpdate[EcsConstants.InitialEntityQueryEntityCapacity];
        protected int _pendingUpdateCount = 0;

        internal protected EntityQueryBase(World world)
        {
            World = world;
        }

        public void AddEntityAddedListener(OnEntityAddedCallback callback)
        {
            for (int i = 0; i < this._onEntityAddedCallbackCount; i++)
            {
                if (this._onEntityAddedCallbacks[i] == callback)
                {
                    throw new Exception("Callback already added.");
                }
            }

            if (this._onEntityAddedCallbackCount == this._onEntityAddedCallbacks.Length)
            {
                Array.Resize(ref this._onEntityAddedCallbacks, 2 * this._onEntityAddedCallbackCount);
            }

            this._onEntityAddedCallbacks[this._onEntityAddedCallbackCount++] = callback;
        }

        public bool RemoveEntityAddedListener(OnEntityAddedCallback callback)
        {
            for (int i = 0; i < this._onEntityAddedCallbackCount; i++)
            {
                if (this._onEntityAddedCallbacks[i] == callback)
                {
                    this._onEntityAddedCallbackCount--;

                    // Can't swap with last because listeners are ordered.
                    Array.Copy(this._onEntityAddedCallbacks, i + 1, this._onEntityAddedCallbacks, i, this._onEntityAddedCallbackCount - i);

                    return true;
                }
            }
            
            // Not found.
            return false;
        }

        public void AddEntityRemovedListener(OnEntityRemovedCallback callback)
        {
            for (int i = 0; i < this._onEntityRemovedCallbackCount; i++)
            {
                if (this._onEntityRemovedCallbacks[i] == callback)
                {
                    throw new Exception("Callback already added.");
                }
            }

            if (this._onEntityRemovedCallbackCount == this._onEntityRemovedCallbacks.Length)
            {
                Array.Resize(ref this._onEntityRemovedCallbacks, 2 * this._onEntityRemovedCallbackCount);
            }

            this._onEntityRemovedCallbacks[this._onEntityRemovedCallbackCount++] = callback;
        }

        public bool RemoveEntityRemovedListener(OnEntityRemovedCallback callback)
        {
            for (int i = 0; i < this._onEntityRemovedCallbackCount; i++)
            {
                if (this._onEntityRemovedCallbacks[i] == callback)
                {
                    this._onEntityRemovedCallbackCount--;

                    // Can't swap with last because listeners are ordered.
                    Array.Copy(this._onEntityRemovedCallbacks, i + 1, this._onEntityRemovedCallbacks, i, this._onEntityRemovedCallbackCount - i);

                    return true;
                }
            }

            // Not found.
            return false;
        }

        protected void InvokeAddListeners(in Entity entity)
        {
            for (int i = 0; i < this._onEntityAddedCallbackCount; i++)
            {
                this._onEntityAddedCallbacks[i].Invoke(entity);
            }
        }

        protected void InvokeRemoveListeners(in Entity entity)
        {
            for (int i = 0; i < this._onEntityRemovedCallbackCount; i++)
            {
                this._onEntityRemovedCallbacks[i].Invoke(entity);
            }
        }

        public Entity GetEntity(int index)
        {
            return _entities[index];
        }

        public int GetEntityCount()
        {
            return _entityCount;
        }

        public bool IsEmpty()
        {
            return _entityCount == 0;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        internal abstract EntityQueryBase Clone();

        internal virtual void CopyTo(EntityQueryBase destQuery)
        {
            // Reference! - queries are bound to a World so this should be fine.
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

            if (destQuery._entities.Length < this._entities.Length)
            {
                Array.Resize(ref destQuery._entities, this._entities.Length);
            }

            for (int i = 0; i < this._entities.Length; i++)
            {
                // Copy since only reference is World and it never changes.
                destQuery._entities[i] = this._entities[i];
            }

            destQuery._entityCount = this._entityCount;

            // ### HEAP ALLOCATION
            destQuery._entityIndexToQueryIndex = new Dictionary<int, int>(this._entityIndexToQueryIndex);

            this._componentIds1.CopyToResize(destQuery._componentIds1);

            if (this._componentIds2 != null)
            {
                this._componentIds2.CopyToResize(destQuery._componentIds2);
            }

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
            this._lockCount++;
        }

        internal void Unlock()
        {
#if DEBUG
            if (this._lockCount == 0)
            {
                throw new InvalidOperationException($"Tried to unlock filter with no locks. Type={GetType().Name}");
            }
#endif

            this._lockCount--;

            if (this._lockCount == 0 && this._pendingUpdateCount > 0)
            {
                ProcessPendingUpdates();
            }
        }

        internal virtual void OnAddIncludeComponent(
            in Entity entity,
            in World.EntityData entityData,
            int componentTypeIndex)
        {
            if (this._entityIndexToQueryIndex.ContainsKey(entity.Id))
            {
                return;
            }

            for (int i = 0; i < this.IncludedComponentTypeIndices.Length; i++)
            {
                if (this.IncludedComponentTypeIndices[i] == componentTypeIndex)
                {
                    continue;
                }

                int entityComponentIndex;

                for (entityComponentIndex = entityData.ComponentCount - 1; entityComponentIndex >= 0; entityComponentIndex--)
                {
                    if (entityData.Components[entityComponentIndex].TypeIndex == this.IncludedComponentTypeIndices[i])
                    {
                        break;
                    }
                }

                if (entityComponentIndex < 0)
                {
                    // One or more included components required to match the filter.
                    return;
                }
            }

            if (QueueEntityUpdate(
                new PendingEntityUpdate
                {
                    Entity = entity,
                    Operation = PendingEntityUpdate.OperationType.Add,
                }))
            {
                return;
            }

            AddEntityToQueryResults(entity);
        }

        internal virtual void OnAddExcludeComponent(
            in Entity entity,
            in World.EntityData entityData,
            int componentTypeIndex)
        {
            if (!this._entityIndexToQueryIndex.ContainsKey(entity.Id))
            {
                return;
            }

            for (int i = 0; i < this.ExcludedComponentTypeIndices.Length; i++)
            {
                if (this.ExcludedComponentTypeIndices[i] == componentTypeIndex)
                {
                    continue;
                }

                for (int j = 0; j < entityData.ComponentCount; j++)
                {
                    if (entityData.Components[j].TypeIndex == this.ExcludedComponentTypeIndices[i])
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
                    Operation = PendingEntityUpdate.OperationType.Remove,
                }))
            {
                return;
            }

            RemoveEntityFromQueryResults(entity);
        }

        internal virtual void OnRemoveIncludeComponent(
            in Entity entity,
            in World.EntityData entityData,
            int componentTypeIndex)
        {
            if (!this._entityIndexToQueryIndex.ContainsKey(entity.Id))
            {
                return;
            }

            if (QueueEntityUpdate(
                new PendingEntityUpdate
                {
                    Entity = entity,
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
            Debug.Assert(!this._entityIndexToQueryIndex.ContainsKey(entity.Id));

            for (int i = 0; i < this.ExcludedComponentTypeIndices.Length; i++)
            {
                if (this.ExcludedComponentTypeIndices[i] == componentTypeIndex)
                {
                    continue;
                }

                for (int j = 0; j < entityData.ComponentCount; j++)
                {
                    if (entityData.Components[j].TypeIndex == this.ExcludedComponentTypeIndices[i])
                    {
                        // At least one excluded component remains.
                        return;
                    }
                }
            }

            // Add this entity from results. No excluded components are attached to the entity.

            if (QueueEntityUpdate(
                new PendingEntityUpdate
                {
                    Entity = entity,
                    Operation = PendingEntityUpdate.OperationType.Add,
                }))
            {
                return;
            }

            AddEntityToQueryResults(entity);
        }

        protected abstract void AddComponentsToResult(Entity entity, int index);

        private bool QueueEntityUpdate(in PendingEntityUpdate pendingUpdate)
        {
            if (_lockCount == 0)
            {
                return false;
            }

            if (this._pendingEntityUpdates.Length == this._pendingUpdateCount)
            {
                Array.Resize(ref this._pendingEntityUpdates, 2 * this._pendingUpdateCount);
            }

            this._pendingEntityUpdates[this._pendingUpdateCount++] = pendingUpdate;

            return true;
        }

        private void AddEntityToQueryResults(in Entity entity)
        {
            if (this._entities.Length == this._entityCount)
            {
                Array.Resize(ref this._entities, 2 * this._entityCount);
                Array.Resize(ref this._componentIds1, 2 * this._entityCount);

                if (this._componentIds2 != null)
                {
                    Array.Resize(ref this._componentIds2, 2 * this._entityCount);
                }
            }

            this._entityIndexToQueryIndex[entity.Id] = this._entityCount;
            this._entities[this._entityCount] = entity;

            AddComponentsToResult(entity, this._entityCount);

            this._entityCount++;

            InvokeAddListeners(entity);
        }

        private void RemoveEntityFromQueryResults(in Entity entity)
        {
            var queryEntityIndex = this._entityIndexToQueryIndex[entity.Id];

            // Move the last item to removed position.
            if (queryEntityIndex < this._entityCount - 1)
            {
                this._entities[queryEntityIndex] = this._entities[this._entityCount - 1];

                this._componentIds1[queryEntityIndex] = this._componentIds1[this._entityCount - 1];

                if (this._componentIds2 != null)
                {
                    this._componentIds2[queryEntityIndex] = this._componentIds2[this._entityCount - 1];
                }
            }

            this._entityIndexToQueryIndex.Remove(entity.Id);

            this._entityCount--;

            InvokeRemoveListeners(entity);
        }

        private void ProcessPendingUpdates()
        {
            for (int i = 0; i < this._pendingUpdateCount; i++)
            {
                ref readonly var pendingUpdate = ref this._pendingEntityUpdates[i];
                ref readonly var entityData = ref this.World.GetEntityData(pendingUpdate.Entity);

                if (pendingUpdate.Operation == PendingEntityUpdate.OperationType.Add)
                {
                    AddEntityToQueryResults(in pendingUpdate.Entity);
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

            this._pendingUpdateCount = 0;
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

        public struct Enumerator : IDisposable
        {
            EntityQueryBase _query;
            private int _current;

            internal Enumerator(EntityQueryBase query)
            {
                this._query = query ?? throw new ArgumentNullException(nameof(query));
                this._current = -1;

                this._query.Lock();
            }

            public int Current => this._current;

            public void Dispose()
            {
                this._query.Unlock();
            }

            public bool MoveNext()
            {
                return ++this._current < this._query._entityCount;
            }
        }

        public struct ChangedEnumerable<T>
            where T : unmanaged
        {
            private readonly EntityQueryBase _query;
            private readonly Version _lastSystemVersion;

            private readonly int[] _componentIds;

            public ChangedEnumerable(EntityQueryBase query, int[] componentIds, in Version lastSystemVersion)
            {
                this._query = query;
                this._componentIds = componentIds ?? throw new ArgumentNullException(nameof(componentIds));
                this._lastSystemVersion = lastSystemVersion;
            }

            public ChangedEnumerator<T> GetEnumerator()
            {
                return new ChangedEnumerator<T>(this._query, this._componentIds, this._lastSystemVersion);
            }
        }

        public struct ChangedEnumerator<T> : IDisposable
            where T : unmanaged
        {
            private EntityQueryBase _query;
            private int _current;

            private Version _lastSystemVersion;

            private readonly int[] _componentIds;
            private readonly ComponentPool<T> _componentPool;

            internal ChangedEnumerator(
                EntityQueryBase query,
                int[] componentIds,
                in Version lastSystemVersion)
            {
                this._query = query ?? throw new ArgumentNullException(nameof(query));
                this._componentIds = componentIds ?? throw new ArgumentNullException(nameof(componentIds));

                this._current = -1;

                this._query.Lock();

                this._lastSystemVersion = lastSystemVersion;
                this._componentPool = _query.World.GetPool<T>();
            }

            public int Current => this._current;

            public void Dispose()
            {
                this._query.Unlock();
            }

            public bool MoveNext()
            {
                while (++this._current < this._query._entityCount)
                {
                    if (this._lastSystemVersion < _componentPool.GetItemVersion(this._componentIds[_current]) ||
                        this._lastSystemVersion == Version.Zero)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }

    public class EntityQuery<IncType1> : EntityQueryBase
        where IncType1 : unmanaged
    {
        private ComponentPool<IncType1> _componentPool;

        internal EntityQuery(World world) 
            : base(world)
        {
            this.IncludedComponentTypeIndices = new[] 
            { 
                ComponentType<IncType1>.Index 
            };

            this._componentPool = world.GetPool<IncType1>();
            this._componentIds1 = new int[EcsConstants.InitialEntityQueryEntityCapacity];
        }

        public ref IncType1 GetSingleton()
        {
            ref var item = ref this._componentPool.Items[_componentIds1[0]];
            item.Version = this.World.State.GlobalVersion;

            return ref item.Item;
        }

        public ref readonly IncType1 GetSingletonReadonly()
        {
            return ref GetReadonly(0);
        }

        public ref IncType1 Get(int index)
        {
            ref var item = ref this._componentPool.Items[_componentIds1[index]];
            item.Version = this.World.State.GlobalVersion;

            return ref item.Item;
        }

        public ref readonly IncType1 GetReadonly(int index)
        {
            return ref this._componentPool.Items[this._componentIds1[index]].Item;
        }

        public ChangedEnumerable<IncType1> GetChangedEnumerator(in Version systemVersion)
        {
            return new ChangedEnumerable<IncType1>(
                this,
                this._componentIds1,
                systemVersion);
        }

        internal override EntityQueryBase Clone()
        {
            var query = new EntityQuery<IncType1>(this.World);

            CopyTo(query);

            return query;
        }

        internal override void CopyTo(EntityQueryBase queryBase)
        {
            base.CopyTo(queryBase);

            var entityQuery = (EntityQuery<IncType1>)queryBase;

            entityQuery._componentPool = this._componentPool;
        }

        protected override void AddComponentsToResult(Entity entity, int index)
        {
            EntityQueryHelper.AddComponentsToResult<IncType1>(entity, index, _componentIds1);
        }

        public class Exclude<ExcType> : EntityQuery<IncType1> 
            where ExcType : unmanaged
        {
            internal Exclude(World world) : base(world)
            {
                this.ExcludedComponentTypeIndices = new[]
                {
                    ComponentType<ExcType>.Index,
                };
            }
        }

        public class Exclude<ExcType1, ExcType2> : EntityQuery<IncType1>
            where ExcType1 : unmanaged
            where ExcType2 : unmanaged
        {
            protected Exclude(World world) : base(world)
            {
                this.ExcludedComponentTypeIndices = new[]
                {
                    ComponentType<ExcType1>.Index,
                    ComponentType<ExcType2>.Index,
                };
            }
        }
    }

    public class EntityQuery<IncType1, IncType2> : EntityQueryBase
        where IncType1 : unmanaged
        where IncType2 : unmanaged
    {
        private ComponentPool<IncType1> _componentPool1;
        private ComponentPool<IncType2> _componentPool2;

        internal EntityQuery(World world) 
            : base(world)
        {
            this.IncludedComponentTypeIndices = new[] 
            { 
                ComponentType<IncType1>.Index,
                ComponentType<IncType2>.Index,
            };

            this._componentPool1 = world.GetPool<IncType1>();
            this._componentPool2 = world.GetPool<IncType2>();
            this._componentIds1 = new int[EcsConstants.InitialEntityQueryEntityCapacity];
            this._componentIds2 = new int[EcsConstants.InitialEntityQueryEntityCapacity];
        }

        public ref IncType1 GetSingleton()
        {
            ref var item = ref this._componentPool1.Items[this._componentIds1[0]];
            item.Version = this.World.State.GlobalVersion;

            return ref item.Item;
        }

        public ref readonly IncType1 GetSingletonReadonly()
        {
            return ref this._componentPool1.Items[_componentIds1[0]].Item;
        }

        public ref IncType1 Get1(int index)
        {
            ref var item = ref this._componentPool1.Items[_componentIds1[index]];
            item.Version = this.World.State.GlobalVersion;

            return ref item.Item;
        }

        public ref readonly IncType1 Get1Readonly(int index)
        {
            return ref this._componentPool1.Items[this._componentIds1[index]].Item;
        }

        public ref IncType2 Get2(int index)
        {
            ref var item = ref this._componentPool2.Items[_componentIds1[index]];
            item.Version = this.World.State.GlobalVersion;

            return ref item.Item;
        }

        public ref readonly IncType2 Get2Readonly(int index)
        {
            return ref this._componentPool2.Items[this._componentIds2[index]].Item;
        }

        public ChangedEnumerable<T> GetChangedEnumerator<T>(in Version systemVersion)
            where T : unmanaged
        {
            int[] componentIds;

            var type = typeof(T);
            if (type == typeof(IncType1))
            {
                componentIds = this._componentIds1;
            }
            else if (type == typeof(IncType2))
            {
                componentIds = this._componentIds2;
            }
            else
            {
                throw new ArgumentException($"Cannot filter on type not in this filter: type={type}");
            }

            return new ChangedEnumerable<T>(
                this,
                componentIds,
                systemVersion);
        }

        internal override EntityQueryBase Clone()
        {
            var query = new EntityQuery<IncType1>(this.World);

            CopyTo(query);

            return query;
        }

        internal override void CopyTo(EntityQueryBase queryBase)
        {
            base.CopyTo(queryBase);

            var entityQuery = (EntityQuery<IncType1, IncType2>)queryBase;

            entityQuery._componentPool1 = this._componentPool1;
            entityQuery._componentPool2 = this._componentPool2;
        }

        protected override void AddComponentsToResult(Entity entity, int index)
        {
            EntityQueryHelper.AddComponentsToResult<IncType1, IncType2>(
                entity,
                index,
                this._componentIds1,
                this._componentIds2);
        }

        public class Exclude<ExcType> : EntityQuery<IncType1, IncType2> 
            where ExcType : unmanaged
        {
            internal Exclude(World world) : base(world)
            {
                this.ExcludedComponentTypeIndices = new[]
                {
                    ComponentType<ExcType>.Index,
                };
            }
        }

        public class Exclude<ExcType1, ExcType2> : EntityQuery<IncType1>
            where ExcType1 : unmanaged
            where ExcType2 : unmanaged
        {
            protected Exclude(World world) : base(world)
            {
                this.ExcludedComponentTypeIndices = new[]
                {
                    ComponentType<ExcType1>.Index,
                    ComponentType<ExcType2>.Index,
                };
            }
        }
    }

    internal static class EntityQueryHelper
    {
        public static void AddComponentsToResult<IncType1>(
            in Entity entity,
            int index,
            int[] componentIds1)
            where IncType1 : unmanaged
        {
            var entityData = entity.World.GetEntityData(entity);

            for (int i = 0; i < entityData.ComponentCount; i++)
            {
                if (entityData.Components[i].TypeIndex == ComponentType<IncType1>.Index)
                {
                    componentIds1[index] = entityData.Components[i].ItemIndex;

                    break;
                }
            }
        }

        public static void AddComponentsToResult<IncType1, IncType2>(
            in Entity entity, 
            int index, 
            int[] componentIds1, 
            int[] componentIds2)
            where IncType1 : unmanaged
            where IncType2 : unmanaged
        {
            var entityData = entity.World.GetEntityData(entity);

            int remainingComponentCount = 2;

            for (int i = 0; i < entityData.ComponentCount; i++)
            {
                if (entityData.Components[i].TypeIndex == ComponentType<IncType1>.Index)
                {
                    componentIds1[index] = entityData.Components[i].ItemIndex;

                    remainingComponentCount--;
                }
                else if (entityData.Components[i].TypeIndex == ComponentType<IncType2>.Index)
                {
                    componentIds2[index] = entityData.Components[i].ItemIndex;

                    remainingComponentCount--;
                }

                if (remainingComponentCount == 0)
                {
                    break;
                }
            }
        }
    }
}

