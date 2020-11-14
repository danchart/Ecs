using Game.Networking;
using System;
using System.Threading.Tasks;

namespace Game.Server.Commands
{
    public sealed class GetWorldTypeCommand : IServerCommand<WorldInstanceId>
    {
        public bool CanExecute(GameServer server) => true;

        public  async Task<WorldInstanceId> ExecuteAsync(GameServer gameServer)
        {
            if (gameServer.)
        }
    }
}
