using Common.Core;
using Game.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server
{
    public class ClientControlPlaneController
    {
        private readonly PlayerConnectionManager _playerConnections;

        private readonly ILogger _logger;

        public ClientControlPlaneController(ILogger logger, PlayerConnectionManager playerConnections)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._playerConnections = playerConnections ?? throw new ArgumentNullException(nameof(playerConnections));
        }

        public bool Syn(PlayerId playerId, uint sequenceKey)
        {
            if (!this._playerConnections.HasPlayer(playerId))
            {
                this._logger.VerboseError($"Client SYN request for non-existent player: Id={playerId}");

                return false;
            }

            ref var connection = ref this._playerConnections[playerId];

            if (connection.ConnectionState != PlayerConnection.ConnectionStateEnum.None)
            {
                this._logger.VerboseError($"Invalid client SYN request for player: Id={playerId}: State={connection.ConnectionState}");

                return false;
            }


        }

        public bool Ack(PlayerId playerId, uint sequenceKey, uint ackKey)
        {

        }

    }
}
