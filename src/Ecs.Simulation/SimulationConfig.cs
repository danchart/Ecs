namespace Ecs.Simulation
{
    public class SimulationConfig
    {
        // in fixed update frames.
        public int SnapShotCount;

        // error epsilon for tick comparisons, in seconds.
        public float TickEpsilon;

        // fixed update delta time, in seconds.
        public float FixedTick;

        public static SimulationConfig Default = new SimulationConfig
        {
            SnapShotCount = 10,
            TickEpsilon = 0.000001f,
        };
    }
}
