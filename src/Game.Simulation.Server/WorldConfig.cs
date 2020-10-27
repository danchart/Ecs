namespace Game.Simulation.Server
{
    public sealed class WorldConfig
    {
        public int PlayerInputMaxFrameCount;

        public static readonly WorldConfig Default = new WorldConfig
        {
            PlayerInputMaxFrameCount = 32,
        };
    }
}
