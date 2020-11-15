using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ecs.Core.Collections
{
    public sealed class EntityMapList<T>
        where T : struct
    {
        private EntityItem[] _entityItems;

        private int _count;

        internal uint _version;

        private readonly Dictionary<int, int> _entityIndexToDataIndex;

        private readonly int ListCapacity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityMapList(int entityCapacity, int listCapacity)
        {
            this._entityItems = new EntityItem[entityCapacity];
            this._entityIndexToDataIndex = new Dictionary<int, int>(entityCapacity);

            this._count = 0;
            this._version = 0;

            this.ListCapacity = listCapacity;
        }

        public bool Contains(in Entity entity) => this._entityIndexToDataIndex.ContainsKey(entity.Id);

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this._count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            this._entityIndexToDataIndex.Clear();
            this._count = 0;
            this._version++;
        }

        public ItemList this[in Entity entity]
        {
            get
            {
                if (!this._entityIndexToDataIndex.ContainsKey(entity.Id))
                {
                    // Allocate this index

                    if (this._count == this._entityItems.Length)
                    {
                        Array.Resize(ref this._entityItems, 2 * this._count);
                    }

                    this._entityIndexToDataIndex[entity.Id] = this._count;
                    this._entityItems[this._count] = new EntityItem(this, ListCapacity)
                    {
                        Entity = entity,
                    };

                    return this._entityItems[this._count++].Items;
                }
                else
                {
                    var entityIndex = this._entityIndexToDataIndex[entity.Id];

                    if (this._entityItems[entityIndex].Entity != entity)
                    {
                        // Different generation, reset the data.

                        this._entityItems[entityIndex].Entity = entity;
                        this._entityItems[entityIndex].Items.Clear();
                    }

                    return this._entityItems[entityIndex].Items;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator
        {
            EntityMapList<T> _mapList;
            private int _current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(EntityMapList<T> mapList)
            {
                this._mapList = mapList ?? throw new ArgumentNullException(nameof(mapList));
                this._current = -1;
            }

            public ref EntityItem Current 
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref this._mapList._entityItems[_current];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                return ++this._current < this._mapList._count;
            }
        }

        public sealed class ItemList
        {
            internal T[] _items;
            internal int _count;

            private uint _version;
            private EntityMapList<T> _parent;

            internal ItemList(EntityMapList<T> parent, int capacity)
            {
                this._items = new T[capacity];
                this._count = 0;
                this._version = parent._version;
                this._parent = parent;
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    {
                        ClearIfOldVersion();

                        return this._count;
                    }
                }
            }

            // If count < 0 you haven't called New().
            public ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref this._items[_count - 1];
            }

            public ref T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref this._items[index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                this._count = 0;
                this._version = _parent._version;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref T New()
            {
                ClearIfOldVersion();

                if (this._items.Length == this._count)
                {
                    Array.Resize(ref this._items, 2 * this._count);
                }

                this._count++;

                return ref this.Current;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator GetEnumerator()
            {
                ClearIfOldVersion();

                return new Enumerator(this);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ClearIfOldVersion()
            {
                if (_parent._version != this._version)
                {
                    Clear();
                }
            }

            public struct Enumerator
            {
                private ItemList _list;
                private int _current;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(ItemList list)
                {
                    this._list = list ?? throw new ArgumentNullException(nameof(list));
                    this._current = -1;
                }

                public ref T Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => ref _list._items[_current];
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    return ++this._current < this._list._count;
                }
            }
        }

        public struct EntityItem
        {
            public Entity Entity;
            public ItemList Items;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityItem(EntityMapList<T> parent, int capacity) : this()
            {
                Items = new ItemList(parent, capacity);
            }
        }
    }
}
