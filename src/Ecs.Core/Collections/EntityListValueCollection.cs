using System;
using System.Collections.Generic;

namespace Ecs.Core
{

    public class EntityListValueCollection<T>
        where T : class
    {
        internal int[] _entityIds;
        internal AppendOnlyList<T>[] _values;

        internal Dictionary<int, int> _entityMap;

        private int _count;

        public EntityListValueCollection(int capacity)
        {
            _entityIds = new int[capacity];
            _values = new AppendOnlyList<T>[capacity];
            _entityMap = new Dictionary<int, int>(capacity);

            _count = 0;
        }

        public bool Contains(Entity entity)
        {
            return _entityMap.ContainsKey(entity.Id);
        }

        public AppendOnlyList<T> this[Entity entity]
        {
            get
            {
                if (!_entityMap.ContainsKey(entity.Id))
                {
                    if (_entityIds.Length == _count)
                    {
                        Array.Resize(ref _entityIds, 2 * _entityIds.Length);
                        Array.Resize(ref _values, 2 * _values.Length);
                    }

                    _entityMap[entity.Id] = _count;
                    _entityIds[_count] = _count;

                    if (_values[_count] == null)
                    {
                        _values[_count] = new AppendOnlyList<T>(8);
                    }
                    else
                    {
                        _values[_count].Clear();
                    }

                    _count++;
                }

                return _values[_entityMap[entity.Id]];
            }

            //set
            //{
            //    if (_entityMap.ContainsKey(entity.Id))
            //    {
            //        _values.Items[_entityMap[entity.Id]] = value;
            //    }
            //    else
            //    {
            //        if (_entityIds.Length == _count)
            //        {
            //            Array.Resize(ref _entityIds, 2 * _entityIds.Length);
            //        }

            //        _entityMap[entity.Id] = _count;
            //        _values.Add(value);
            //        _entityIds[_count] = _count;

            //        _count++;
            //    }
            //}
        }

        public void Clear()
        {
            this._entityMap.Clear();
            this._count = 0;
        }

        public Enumerator<T> GetEnumerator()
        {
            return new Enumerator<T>(this);
        }

        public struct Enumerator<T>
            where T : class
        {
            private readonly EntityListValueCollection<T> _collection;

            private int _current;

            internal Enumerator(EntityListValueCollection<T> collection)
            {
                this._collection = collection;
                this._current = -1;
            }

            public AppendOnlyList<T> Current => this._collection._values.Items[_current];


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
