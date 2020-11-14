using Common.Core;
using System.Threading.Tasks;

namespace Game.Server
{
    public sealed class SpawnWorldCommand : IServerCommand<GameWorld>
    {
        WorldType _worldType;

        public SpawnWorldCommand(WorldType worldType)
        {
            _worldType = worldType;
        }

        public bool CanExecute(GameServer server)
        {
            return true;
        }

        public async Task<GameWorld> ExecuteAsync(GameServer gameServer)
        {
            return gameServer.SpawnWorld(_worldType);
        }
    }
}
