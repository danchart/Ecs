using Ecs.Core;

namespace Game.Server
{
    public interface IGameWorldLoader
    {
        bool LoadWorld(World world);
    }

    class GameWorldLoader
    {
    }
}
