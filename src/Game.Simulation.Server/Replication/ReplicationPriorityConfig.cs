namespace Game.Simulation.Server
{
    public class ReplicationPriorityConfig
    {
        public float
            DistanceSquardRing0 = 12.0f,
            DistanceSquardRing1 = 24.0f,
            DistanceSquardRing2 = 48.0f;

        public float
            Ring0Priority = 1.0f,
            Ring1Priority = 0.9f,
            Ring2Priority = 0.5f,
            Ring3Priority = 0.1f;

        /// <summary>
        /// Number of ticks per queue priority index to queue replication packet. 
        /// 
        /// 0 is highest and Length - 1 is lowest priority.
        /// </summary>
        public int[] QueueTicks = new int[]
        {
            // 0 - Highest priority, 0 ticks
            0,
            // 1
            4,
            // 2
            16,
            // 3
            64
        };
    }
}
