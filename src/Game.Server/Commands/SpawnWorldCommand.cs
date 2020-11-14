using Common.Core;
using Game.Networking;
using System.Threading.Tasks;

namespace Game.Server
{
    public sealed class SpawnWorldCommand : IServerCommand<WorldInstanceId>
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

        public async Task<WorldInstanceId> ExecuteAsync(GameServer gameServer)
        {
            return gameServer.SpawnWorld(_worldType);
        }
    }
}
