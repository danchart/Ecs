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

        public ref AppendOnlyList<T> this[Entity entity]
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
                    this._entityItems[_count] = new EntityItem
                    {
                        Items = new AppendOnlyList<T>(ListCapacity),
                        Entity = entity,
                    };

                    return ref _entityItems[_count++].Items;
                }
                else
                {
                    var entityIndex = this._entityIndexToDataIndex[entity.Id];

                    if (this._entityItems[entityIndex].Entity != entity)
                    {
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

        public struct EntityItem
        {
            public Entity Entity;
            public AppendOnlyList<T> Items;
        }
    }
}
