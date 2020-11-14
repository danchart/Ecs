using Common.Core;
using System.Threading.Tasks;

namespace Game.Server
{
    public sealed class GetWorldByTypeCommand : IServerCommand<GameWorld>
    {
        readonly bool _createIfNeeded;
        readonly WorldType _worldType;

        public GetWorldByTypeCommand(WorldType worldType, bool createIfNeeded = false)
        {
            _worldType = worldType;
            _createIfNeeded = createIfNeeded;
        }

        public bool CanExecute(GameServer server) => true;

        public async Task<GameWorld> ExecuteAsync(GameServer gameServer)
        {
            foreach (var world in gameServer.GetWorlds())
            {
                if (world.WorldType == _worldType)
                {
                    return world;
                }
            }

            if (_createIfNeeded)
            {
                return gameServer.SpawnWorld(_worldType);
            }

            return null;
        }
    }
}
