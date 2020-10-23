using System;

namespace Game.Simulation.Server
{
    internal sealed class PlayerReplicationDataPool
    {
        private PlayerReplicationData[] _items;
        private int[] _freeItemIndices;

        private int _itemCount = 0;
        private int _freeItemCount = 0;

        private readonly ReplicationConfig _replicationConfig;

        internal PlayerReplicationDataPool(ReplicationConfig replicationConfig, int capacity)
        {
            this._replicationConfig = replicationConfig ?? throw new ArgumentNullException(nameof(replicationConfig));
            this._items = new PlayerReplicationData[capacity];
            this._freeItemIndices = new int[capacity];
        }

        public int Count => this._itemCount - this._freeItemCount;

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
                if (this._itemCount == this._items.Length)
                {
                    Array.Resize(ref this._items, this._itemCount * 2);
                }

                // Current count is new id, then increment.
                newPoolIndex = this._itemCount++;

                // Allocate object if needed, clear otherwise.
                if (this._items[newPoolIndex] == null)
                {
                    this._items[newPoolIndex] = new PlayerReplicationData(
                        capacity: this._replicationConfig.Capacity.InitialReplicatedEntityCapacity,
                        componentCapacity: this._replicationConfig.Capacity.InitialReplicatedComponentCapacity,
                        tickTime: this._replicationConfig.Networking.PriorityQueueDelayBaseTick,
                        queueTicks: this._replicationConfig.Networking.PriorityQueueDelay);
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
            // Clear item data
            this._items[index] = default;

            // Resize free item pool if out-of-space
            if (this._freeItemCount == this._freeItemIndices.Length)
            {
                Array.Resize(ref this._freeItemIndices, this._freeItemCount * 2);
            }

            // Add free index to free item pool
            this._freeItemIndices[this._freeItemCount++] = index;
        }

        public ref PlayerReplicationData GetItem(int index)
        {
            return ref this._items[index];
        }
    }
}
