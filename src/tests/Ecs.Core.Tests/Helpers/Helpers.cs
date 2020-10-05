namespace Ecs.Core.Tests
{
    public static class Helpers
    {
        public static World NewWorld()
        {
            return new World(EcsConfig.Default);
        }
    }
}
