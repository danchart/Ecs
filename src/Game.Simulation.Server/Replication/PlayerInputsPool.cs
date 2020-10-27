using System;

namespace Game.Simulation.Server
{
    internal sealed class PlayerInputsPool
    {
        private PlayerInputs[] _items;
        private int[] _freeItemIndices;

        private int _count = 0;
        private int _freeItemCount = 0;

        private readonly int _inputFrameCount;

        internal PlayerInputsPool(int inputFrameCount, int capacity)
        {
            this._inputFrameCount = inputFrameCount;
            this._items = new PlayerInputs[capacity];
            this._freeItemIndices = new int[capacity];
        }

        public int Count => this._count - this._freeItemCount;

        public int New()
        {
            int newPoolIndex;

            // Use free pool indices first
            if (this._freeItemCount > 0)
            {
                newPoolIndex = this._freeItemIndices[--this._freeItemCount];
            }
            else
            {
                // Resize pool when out-of-space
                if (this._count == this._items.Length)
                {
                    Array.Resize(ref this._items, this._count * 2);
                    Array.Resize(ref this._freeItemIndices, this._count * 2);
                }

                // Current count is new id, then increment.
                newPoolIndex = this._count++;

                // Allocate object if needed, clear otherwise.
                if (this._items[newPoolIndex] == null)
                {
                    this._items[newPoolIndex] = new PlayerInputs(this._inputFrameCount);
                }
                else
                {
                    this._items[newPoolIndex].Clear();
                }
            }

            return newPoolIndex;
        }

        public void Free(int index)
        {
            // Add free index to free item pool
            this._freeItemIndices[this._freeItemCount++] = index;

            // Clear item data
            this._items[index] = default;
        }

        public PlayerInputs Get(int index) => this._items[index];
    }
}
