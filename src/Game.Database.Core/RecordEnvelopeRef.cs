namespace Game.Database.Core
{
    public readonly struct RecordEnvelopeRef<TRecord>
        where TRecord : struct
    {
        public readonly int Index;
        public readonly RecordEnvelopePool<TRecord> Pool;

        internal RecordEnvelopeRef(int index, RecordEnvelopePool<TRecord> pool)
        {
            this.Index = index;
            this.Pool = pool;
        }

        public ref RecordEnvelope<TRecord> Unref() => ref this.Pool.Get(this.Index);
    }
}
