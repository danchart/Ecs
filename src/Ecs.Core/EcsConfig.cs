namespace Ecs.Core
{
    public struct EcsConfig
    {
        public int InitialEntityPoolCapacity;
        public int InitialComponentPoolCapacity;
        public int InitialEntityComponentCapacity;
        public int InitialEntityQueryCapacity;
        public int InitialComponentToEntityQueryMapCapacity;

        public static readonly EcsConfig Default = new EcsConfig
        {
            InitialEntityPoolCapacity = 256,
            InitialComponentPoolCapacity = 256,
            InitialEntityComponentCapacity = 8,

            InitialEntityQueryCapacity = 64,
            InitialComponentToEntityQueryMapCapacity = 256,
        };
    }
}
