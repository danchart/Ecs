using System;
using System.Collections.Generic;

namespace Ecs.Core.Collections
{
    public class EntityMapList<T>
        where T : struct
    {
        private EntityItem[] _entityItems;

        private Dictionary<int, int> _entityIndexToDataIndex;

        private int _count;

        private readonly int ListCapacity;

        public EntityMapList(int entityCapacity, int listCapacity)
        {
            this._entityItems = new EntityItem[entityCapacity];
            this._entityIndexToDataIndex = new Dictionary<int, int>(entityCapacity);

            this._count = 0;

            this.ListCapacity = listCapacity;
        }

        public int Count => this._count;

        public void Clear()
        {
            this._entityIndexToDataIndex.Clear();
            this._count = 0;
        }

        public ref ItemList this[Entity entity]
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
                    this._entityItems[_count] = new EntityItem(ListCapacity)
                    {
                        Entity = entity,
                    };

                    return ref _entityItems[_count++].Items;
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

                    return ref this._entityItems[entityIndex].Items;
                }
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator
        {
            EntityMapList<T> _mapList;
            private int _current;

            internal Enumerator(EntityMapList<T> mapList)
            {
                this._mapList = mapList ?? throw new ArgumentNullException(nameof(mapList));
                this._current = -1;
            }

            public EntityItem Current => _mapList._entityItems[_current];

            public bool MoveNext()
            {
                return ++this._current < this._mapList._count;
            }
        }

        public class ItemList
        {
            internal T[] _items;
            internal int _count;

            internal ItemList(int capacity)
            {
                this._items = new T[capacity];
                this._count = 0;
            }

            public int Count => this._count;

            // If count < 0 you haven't called New().
            public ref T Current => ref this._items[_count-1];

            public ref T this[int index] => ref this._items[index];

            public void Clear()
            {
                _count = 0;
            }

            public ref T New()
            {
                if (this._items.Length == this._count)
                {
                    Array.Resize(ref this._items, 2 * this._count);
                }

                this._count++;

                return ref this.Current;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(this);
            }

            public struct Enumerator
            {
                private ItemList _list;
                private int _current;

                internal Enumerator(ItemList list)
                {
                    this._list = list ?? throw new ArgumentNullException(nameof(list));
                    this._current = -1;
                }

                public T Current => _list._items[_current];

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

            public EntityItem(int capacity) : this()
            {
                Items = new ItemList(capacity);
            }
        }
    }
}
