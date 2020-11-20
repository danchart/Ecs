using Common.Core;
using Game.Networking;
using Game.Simulation.Server;
using System.Threading.Tasks;

namespace Game.Server
{
    public sealed class ConnectPlayerServerCommand : IServerCommand<PlayerConnectionRef>
    {
        readonly WorldInstanceId _instanceId;
        readonly PlayerId _playerId;
        readonly byte[] _encryptionKey;

        public ConnectPlayerServerCommand(
            WorldInstanceId instanceId,
            PlayerId playerId,
            byte[] encryptionKey)
        {
            _instanceId = instanceId;
            _playerId = playerId;
            _encryptionKey = encryptionKey;
        }

        public bool CanExecute(GameServer server) => true;

        public async Task<PlayerConnectionRef> ExecuteAsync(GameServer gameServer)
        {
            return gameServer.ConnectPlayer(_instanceId, _playerId, _encryptionKey);
        }
    }
}
