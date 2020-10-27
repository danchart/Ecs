using Game.Networking;
using Game.Networking.PacketData;
using System;

namespace Game.Simulation.Server
{

    internal sealed class PlayerInputs
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

        public ref Item this[int index] => ref this._items[index];

        public int MoveNext()
        {
            if (this._count  == this._items.Length)
            {
                return -1;
            }

            var index = (this._index + this._count) % this._items.Length;

            this._count++;

            return index;
        }

        public void RemoveFirst()
        {
            this._index = (this._index + 1) % this._items.Length;
            this._count--;
        }

        public struct Item
        {
            public FrameIndex FrameIndex;
            public PlayerInputData Input;
        }
    }
}
