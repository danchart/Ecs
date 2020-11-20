namespace Game.Simulation.Core
{
    public sealed class PlayerInputConfig
    {
        public int MaxFrameCount;

        public static readonly PlayerInputConfig Default = new PlayerInputConfig
        {
            MaxFrameCount = 32,
        };
    }
}
