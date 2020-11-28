using Game.Networking;
using Game.Networking.PacketData;
using System;

namespace Game.Simulation.Server
{
    public sealed class PlayerInputs
    {
        private Item[] _items;

        private int _index;
        private int _count;

        internal PlayerInputs(int capacity)
        {
            this._items = new Item[capacity];
            this._index = 0;
            this._count = 0;
        }

        public int Count => this._count;

        public ref Item GetFirst() => ref this._items[this._index];
        public ref Item GetLast() => ref this._items[this._index];

        public ref Item GetNext()
        {
            if (this._count == this._items.Length)
            {
                // Not auto-expanding the array, should we?
                throw new IndexOutOfRangeException($"Out of player input capacity: capacity={this._items.Length}");
            }

            var index = (this._index + this._count) % this._items.Length;

            this._count++;

            return ref this._items[index];
        }

        public void RemoveFirst()
        {
            this._index = (this._index + 1) % this._items.Length;
            this._count--;
        }

        public void Clear()
        {
            this._index = 0;
            this._count = 0;
        }

        public struct Item
        {
            public FrameNumber FrameIndex;
            public InputData Input;
        }
    }
}
