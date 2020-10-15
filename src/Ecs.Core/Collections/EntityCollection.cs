using System;
using System.Collections.Generic;

namespace Ecs.Core
{

    public class EntityCollection<T>
        where T : class
    {
        internal int[] _entityIds;
        internal AppendOnlyList<T> _values;

        internal Dictionary<int, int> _entityMap;

        private int _count;

        public EntityCollection(int capacity)
        {
            _entityIds = new int[capacity];
            _values = new AppendOnlyList<T>(capacity);
            _entityMap = new Dictionary<int, int>(capacity);

            _count = 0;
        }

        public bool Contains(Entity entity)
        {
            return _entityMap.ContainsKey(entity.Id);
        }

        public T this[Entity entity]
        {
            get
            {
                if (!_entityMap.ContainsKey(entity.Id))
                {
                    return null;
                }

                return _values.Items[_entityMap[entity.Id]];
            }

            set
            {
                if (_entityMap.ContainsKey(entity.Id))
                {
                    _values.Items[_entityMap[entity.Id]] = value;
                }
                else
                {
                    if (_entityIds.Length == _count)
                    {
                        Array.Resize(ref _entityIds, 2 * _entityIds.Length);
                    }

                    _entityMap[entity.Id] = _count;
                    _values.Add(value);
                    _entityIds[_count] = _count;

                    _count++;
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _values.Count; i++)
            {

            }

            this._hasValue.Clear();
        }

        public IndexEnumerator<T> GetEnumerator()
        {
            return new IndexEnumerator<T>(this);
        }

        public struct IndexEnumerator<T>
            where T : class
        {
            private readonly EntityCollection<T> _collection;

            private int _current;

            internal IndexEnumerator(EntityCollection<T> collection)
            {
                this._collection = collection;
                this._current = -1;
            }

            public T Current => this._collection._values.Items[_current];


            public bool MoveNext()
            {
                while (
                    !_collection._hasValue.Contains(++this._current) &&
                    this._current < this._collection._values.Count)
                {
                }

                return this._current < this._collection._values.Count;
            }
        }
    }


#if MOTHBALL
    /// <summary>
    /// Very fast at storing entities at the cost of size.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntityCollection<T>
        where T : class
    {
        internal HashSet<int> _hasValue;
        internal AppendOnlyList<T> _values;

        public EntityCollection(int capacity)
        {
            _hasValue = new HashSet<int>(capacity);
            _values = new AppendOnlyList<T>(capacity);
        }

        public bool Contains(Entity entity)
        {
            return _hasValue.Contains(entity.Id);
        }

        public T this[Entity entity]
        {
            get
            {
                if (!_hasValue.Contains(entity.Id))
                {
                    return null;
                }

                return _values.Items[entity.Id];
            }

            set
            {
                _values.Items[entity.Id] = value;
            }
        }

        public void Clear()
        {
            this._hasValue.Clear();
        }

        public IndexEnumerator<T> GetEnumerator()
        {
            return new IndexEnumerator<T>(this);
        }

        public struct IndexEnumerator<T>
            where T : class
        {
            private readonly EntityCollection<T> _collection;

            private int _current;

            internal IndexEnumerator(EntityCollection<T> collection)
            {
                this._collection = collection;
                this._current = -1;
            }

            public T Current => this._collection._values.Items[_current];


            public bool MoveNext()
            {
                while (
                    !_collection._hasValue.Contains(++this._current) &&
                    this._current < this._collection._values.Count)
                {
                }

                return this._current < this._collection._values.Count;
            }
        }
    }
#endif
}
