namespace Game.Database.Core
{
    /// <summary>
    /// Wraps all DB entities for read or write.
    /// </summary>
    public struct RecordEnvelope<TRecord>
        where TRecord : struct
    {
        public TRecord Record;

        public uint ETag;
    }
}
