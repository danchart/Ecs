namespace Common.Core
{
    public class ByteArrayPool
    {
        public readonly int ArraySize;
        public readonly int PoolCapacity;

        private readonly byte[][] _data;

        private int _count;

        private int[] _freeIndices;
        private int _freeCount;

        public ByteArrayPool(int arraySize, int poolCapacity)
        {
            this.ArraySize = arraySize;
            this.PoolCapacity = poolCapacity;

            this._data = new byte[poolCapacity][];

            for (int i = 0; i < this._data.Length; i++)
            {
                this._data[i] = new byte[arraySize];
            }

            this._freeIndices = new int[poolCapacity];

            this._count = 0;
            this._freeCount = 0;
        }

        public int Count => this._count - this._freeCount;

        public int New()
        {
            if (_freeCount > 0)
            {
                return --this._freeCount;
            }

            if (this._count == this.PoolCapacity)
            {
                return -1;
            }

            return this._count++;
        }

        public byte[] GetBuffer(int index)
        {
            return this._data[index];
        }

        public void Free(int index)
        {
            this._freeIndices[this._freeCount++] = index;
        }
    }
}
