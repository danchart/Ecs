using System;

namespace Common.Core
{
    public class FixedIndexDictionary<TValue>
        where TValue : unmanaged
    {
        private VersionedValue[] _items;
        private uint _version;

        private const uint FirstVersion = 1U;

        public FixedIndexDictionary(int size)
        {
            this._items = new VersionedValue[size];
            this._version = FirstVersion;
        }

        public ref TValue this[int index]
        {
            get
            {
                _items[index].Version = this._version;

                return ref _items[index].Value;
            }
        }

        public void Clear()
        {
            if (unchecked(++this._version) == 0)
            {
                // On rollover clear the data.
                Array.Clear(_items, 0, _items.Length);
                this._version = FirstVersion;
            }
        }
        
        public bool ContainsKey(int index) => _items[index].Version == this._version;

        private struct VersionedValue
        {
            public TValue Value;
            public uint Version;
        }
    }
}
