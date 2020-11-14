using Common.Core;
using Game.Networking;
using System.Net;
using System.Threading.Tasks;

namespace Game.Server
{
    public sealed class ConnectPlayerCommand : IServerCommand<bool>
    {
        WorldInstanceId _instanceId;
        PlayerId _playerId;
        byte[] _encryptionKey;
        IPEndPoint _ipEndPoint;

        public ConnectPlayerCommand(
            WorldInstanceId instanceId,
            PlayerId playerId,
            byte[] encryptionKey,
            IPEndPoint ipEndPoint)
        {
            _instanceId = instanceId;
            _playerId = playerId;
            _encryptionKey = encryptionKey;
            _ipEndPoint = ipEndPoint;
        }

        public bool CanExecute(GameServer server) => true;

        public async Task<bool> ExecuteAsync(GameServer gameServer)
        {
            return gameServer.ConnectPlayer(_instanceId, _playerId, _encryptionKey, _ipEndPoint);
        }
    }
}
