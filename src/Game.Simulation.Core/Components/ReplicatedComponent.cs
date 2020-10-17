namespace Game.Simulation.Core
{
    public struct ReplicatedComponent
    {
        /// <summary>
        /// Intrinsic replication relevance for this entity.
        /// </summary>
        public ReplicationRelevance Relevance;
    }

    public enum ReplicationRelevance
    {
        Low,
        Normal,
        High
    }
}
