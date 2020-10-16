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
    }
}
