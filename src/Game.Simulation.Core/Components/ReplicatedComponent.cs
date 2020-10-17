namespace Game.Simulation.Core
{
    public struct ReplicatedComponent
    {
        /// <summary>
        /// Intrinsic replication relevance for this entity.
        /// </summary>
        public PriorityEnum BasePriority;
    }

    public enum PriorityEnum
    {
        Low,
        Normal,
        High
    }
}
