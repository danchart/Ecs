namespace Ecs.Core
{
    public struct EcsConfig
    {
        public int InitialEntityPoolSize;
        public int InitialComponentPoolSize;
        public int InitialEntityComponentCount;

        public static readonly EcsConfig Default = new EcsConfig
        {
            InitialEntityPoolSize = 256,
            InitialComponentPoolSize = 256,
            InitialEntityComponentCount = 8,
        };
    }
}
