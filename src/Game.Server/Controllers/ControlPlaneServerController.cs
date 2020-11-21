using Common.Core;
using Game.Networking;
using Game.Simulation.Server;
using System;
using System.Net;

namespace Game.Server
{
    public class ControlPlaneServerController
    {
        private ServerPacketEnvelope _serverPacket;

        private readonly OutgoingServerChannel _channelOutgoing;
        private readonly PlayerConnectionManager _playerConnections;

        private readonly GameWorlds _worlds;

        private readonly ILogger _logger;

        public ControlPlaneServerController(
            ILogger logger, 
            PlayerConnectionManager playerConnections,
            OutgoingServerChannel channelOutgoing,
            GameWorlds worlds)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._playerConnections = playerConnections ?? throw new ArgumentNullException(nameof(playerConnections));
            this._channelOutgoing = channelOutgoing ?? throw new ArgumentNullException(nameof(channelOutgoing));
            this._worlds = worlds ?? throw new ArgumentNullException(nameof(worlds));
        }

        public bool Process(PlayerId playerId, IPEndPoint endPoint, in ControlPacket controlPacket)
        {
            switch (controlPacket.ControlMessage)
            {
                case ControlMessageEnum.ConnectSyn:

                    return SynHandshake(playerId, controlPacket.ControlSynPacketData.SequenceKey);

                case ControlMessageEnum.ConnectAck:

                    return AckHandshake(
                        playerId,
                        controlPacket.ControlAckPacketData.SequenceKey,
                        controlPacket.ControlAckPacketData.AcknowledgementKey,
                        endPoint);
            }

            return false;
        }

        public bool SynHandshake(PlayerId playerId, uint sequenceKey)
        {
            if (!this._playerConnections.HasPlayer(playerId))
            {
                this._logger.VerboseError($"Client SYN request for non-existent player: Id={playerId}");

                return false;
            }

            ref var connection = ref this._playerConnections[playerId];

            if (connection.State != PlayerConnection.ConnectionState.PreConnected ||
                connection.State != PlayerConnection.ConnectionState.Connecting)
            {
                this._logger.VerboseError($"Invalid player state for client SYN request: state={connection.State}, expectedState={PlayerConnection.ConnectionState.PreConnected}, {PlayerConnection.ConnectionState.Connecting}");

                return false;
            }

            // Send SYN-ACK
            
            connection.Handshake.AcknowledgementKey = ConnectionHandshakeKeys.NewAcknowledgementKey();
            connection.State = PlayerConnection.ConnectionState.Connecting;

            this._serverPacket.Type = ServerPacketType.Control;
            this._serverPacket.PlayerId = connection.PlayerId;
            this._serverPacket.ControlPacket.ControlAckPacketData.SequenceKey = sequenceKey;
            this._serverPacket.ControlPacket.ControlAckPacketData.AcknowledgementKey = connection.Handshake.AcknowledgementKey;

            this._channelOutgoing.SendClientPacket(connection.EndPoint, in this._serverPacket);

            return true;
        }

        public bool AckHandshake(PlayerId playerId, uint sequenceKey, uint ackKey, IPEndPoint endPoint)
        {
            if (!this._playerConnections.HasPlayer(playerId))
            {
                this._logger.VerboseError($"Client SYN request for non-existent player: Id={playerId}");

                return false;
            }

            ref var connection = ref this._playerConnections[playerId];

            if (connection.State != PlayerConnection.ConnectionState.Connecting)
            {
                this._logger.VerboseError($"Invalid client SYN request for player: Id={playerId}: State={connection.State}");

                return false;
            }

            if (connection.Handshake.AcknowledgementKey != this._serverPacket.ControlPacket.ControlAckPacketData.AcknowledgementKey)
            {
                this._logger.VerboseError($"Client ACK request incorrect ACK key: Id={playerId}, Key={this._serverPacket.ControlPacket.ControlAckPacketData.AcknowledgementKey}");

                return false;
            }

            // Connected

            connection.State = PlayerConnection.ConnectionState.Connected;
            connection.EndPoint = endPoint;

            return true;
        }
    }
}
