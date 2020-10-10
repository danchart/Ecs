namespace Ecs.Core
{
    public struct EcsConfig
    {
        public int InitialEntityPoolCapacity;
        public int InitialComponentPools;
        public int InitialComponentPoolCapacity;
        public int InitialEntityComponentCapacity;
        public int InitialEntityQueryCapacity;
        public int InitialComponentToEntityQueryMapCapacity;

        public int InitialSystemsCapacity;

        public static readonly EcsConfig Default = new EcsConfig
        {
            InitialEntityPoolCapacity = 256,
            InitialComponentPools = 256,
            InitialComponentPoolCapacity = 128,
            InitialEntityComponentCapacity = 8,

            InitialSystemsCapacity = 4, // Low # of Systems anticipated

            InitialEntityQueryCapacity = 64,
            InitialComponentToEntityQueryMapCapacity = 256,
        };
    }
}
