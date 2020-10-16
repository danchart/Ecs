using System;
using System.Collections.Generic;

namespace Ecs.Core.Collections
{
    /// <summary>
    /// Provides a pooled map-list of entities. Trades more memory for less allocations.
    /// </summary>
    public class MOTHBALL_EntityMapList<T>
        where T : struct
    {
        internal AppendOnlyList<T>[] _listPool;
        internal int _count;

        internal Dictionary<Entity, AppendOnlyList<T>> _entityMapToList;

        private readonly int ListCapacity;

        public MOTHBALL_EntityMapList(int entityCapacity, int listPoolCapacity, int listCapacity)
        {
            ListCapacity = listCapacity;

            this._entityMapToList = new Dictionary<Entity, AppendOnlyList<T>>(entityCapacity);
            this._listPool = new AppendOnlyList<T>[listPoolCapacity];

            AllocateListPool(this._listPool, start: 0, length: this._listPool.Length);

            this._count = 0;
        }

        public int Count()
        {
            return this._entityMapToList.Count;
        }

        public void Clear()
        {
            this._entityMapToList.Clear();

            this._count = 0;
        }

        public AppendOnlyList<T> this[Entity entity]
        {
            get
            {
                if (!this._entityMapToList.ContainsKey(entity))
                {
                    if (this._count == this._listPool.Length)
                    {
                        Array.Resize(ref _listPool, 2 * this._count);

                        AllocateListPool(this._listPool, start: this._count, length: this._count);
                    }

                    // Assign and clear the next available list.
                    this._entityMapToList[entity] = _listPool[_count++];
                    this._entityMapToList[entity].Clear();
                }

                return this._entityMapToList[entity];
            }
        }

        public Dictionary<Entity, AppendOnlyList<T>>.Enumerator GetEnumerator()
        {
            return this._entityMapToList.GetEnumerator();
        }

        private void AllocateListPool(AppendOnlyList<T>[] listPool, int start, int length)
        {
            for (int i = start; i < start + length; i++)
            {
                this._listPool[i] = new AppendOnlyList<T>(this.ListCapacity);
            }
        }
    }
}
