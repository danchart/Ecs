using System;
using System.Runtime.CompilerServices;

namespace Common.Core
{
    public class FixedIndexDictionary<TValue>
        where TValue : unmanaged
    {
        private VersionedValue[] _items;
        private uint _version;

        private const uint FirstVersion = 1U;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedIndexDictionary(int size)
        {
            this._items = new VersionedValue[size];
            this._version = FirstVersion;
        }

        public ref TValue this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _items[index].Version = this._version;

                return ref _items[index].Value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (unchecked(++this._version) == 0)
            {
                // On rollover clear the data.
                Array.Clear(_items, 0, _items.Length);
                this._version = FirstVersion;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(int index) => _items[index].Version == this._version;

        private struct VersionedValue
        {
            public TValue Value;
            public uint Version;
        }
    }
}
