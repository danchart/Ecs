using System;

namespace Database.Core
{
    public class RecordEnvelopePool<TRecord>
        where TRecord : struct
    {
        private RecordEnvelope<TRecord>[] _items;
        private int _count;

        private int[] _freeIndices;
        private int _freeItemCount;

        public RecordEnvelopePool(int capacity)
        {
            this._items = new RecordEnvelope<TRecord>[capacity];
            this._count = 0;

            this._freeIndices = new int[capacity];
            this._freeItemCount = 0;
        }

        public ref RecordEnvelope<TRecord> Get(int index) => ref this._items[index];

        public RecordEnvelopeRef<TRecord> Ref(int index) => new RecordEnvelopeRef<TRecord>(index, this);

        public int New()
        {
            if (this._freeItemCount > 0)
            {
                return this._freeIndices[--this._freeItemCount];
            }

            if (this._count == this._items.Length)
            {
                Array.Resize(ref this._items, 2 * this._count);
            }

            return this._count++;
        }

        public void Free(int index)
        {
            this._freeIndices[this._freeItemCount++] = index;
        }
    }
}
