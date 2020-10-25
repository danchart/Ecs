namespace Game.Networking
{
    public class PlayerConnectionConfig
    {
        public static readonly PlayerConnectionConfig Default = new PlayerConnectionConfig();

        public CapacityConfig Capacity = new CapacityConfig();

        public class CapacityConfig
        {
            public int InitialConnectionsCapacity = 16;
        }
    }
}
