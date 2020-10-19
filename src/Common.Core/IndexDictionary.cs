using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Common.Core
{
    public class IndexDictionary<T>
        where T : unmanaged
    {
        private T[] _items;
        private int[] _freeIndices;

        private int _count;
        private int _freeCount;

        private Dictionary<int, int> _idToIndex;

        public IndexDictionary(int capacity)
        {
            this._items = new T[capacity];
            this._freeIndices = new int[capacity];
            this._idToIndex = new Dictionary<int, int>(capacity);

            this._count = 0;
            this._freeCount = 0;
        }

        public int Count
        {
            get => this._count;
        }

        public ref T this[int playerId]
        {
            get => ref this._items[_idToIndex[playerId]];
        }

        public void Add(int key, in T value)
        {
            Debug.Assert(!this._idToIndex.ContainsKey(key));

            int index;

            if (this._freeCount > 0)
            {
                index = this._freeIndices[--this._freeCount];
            }
            else
            {
                if (this._count == this._items.Length)
                {
                    Array.Resize(ref _items, 2 * this._count);
                    Array.Resize(ref _freeIndices, 2 * this._count);
                }

                index = this._count++;
            }

            this._idToIndex[key] = index;
            this._items[index] = value;
        }

        public void Remove(int playerId)
        {
            this._count--;

            this._freeIndices[_freeCount++] = this._idToIndex[playerId];
            this._idToIndex.Remove(playerId);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator
        {
            private readonly IndexDictionary<T> _parent;

            private readonly Dictionary<int, int>.Enumerator _enumerator;

            internal Enumerator(IndexDictionary<T> parent)
            {
                this._parent = parent;

                this._enumerator = parent._idToIndex.GetEnumerator();
            }

            public ref T Current
            {
                get => ref this._parent._items[this._enumerator.Current.Value];
            }

            public bool MoveNext()
            {
                return this._enumerator.MoveNext();
            }
        }
    }
}
