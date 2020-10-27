using Common.Core;
using Game.Networking;
using System;
using System.Net;

namespace Game.Server
{
    public class ControlPacketController
    {
        private ServerPacketEnvelope _serverPacket;

        private readonly ServerChannelOutgoing _channelOutgoing;
        private readonly PlayerConnectionManager _playerConnections;

        private readonly ILogger _logger;

        public ControlPacketController(
            ILogger logger, 
            PlayerConnectionManager playerConnections,
            ServerChannelOutgoing channelOutgoing)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._playerConnections = playerConnections ?? throw new ArgumentNullException(nameof(playerConnections));
            this._channelOutgoing = channelOutgoing ?? throw new ArgumentNullException(nameof(channelOutgoing));
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

            if (connection.State == PlayerConnection.ConnectionState.Connected)
            {
                this._logger.VerboseError($"Client SYN request for connected player: Id={playerId}");

                return false;
            }

            // Send SYN-ACK
            
            connection.HandshakeKeys.AcknowledgementKey = NewAcknowledgementKey();
            connection.State = PlayerConnection.ConnectionState.Connecting;

            this._serverPacket.Type = ServerPacketType.Control;
            this._serverPacket.PlayerId = connection.PlayerId;
            this._serverPacket.ControlPacket.ControlAckPacketData.SequenceKey = sequenceKey;
            this._serverPacket.ControlPacket.ControlAckPacketData.AcknowledgementKey = connection.HandshakeKeys.AcknowledgementKey;

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

            if (connection.HandshakeKeys.AcknowledgementKey != this._serverPacket.ControlPacket.ControlAckPacketData.AcknowledgementKey)
            {
                this._logger.VerboseError($"Client ACK request incorrect ACK key: Id={playerId}, Key={this._serverPacket.ControlPacket.ControlAckPacketData.AcknowledgementKey}");

                return false;
            }

            // Connected

            connection.State = PlayerConnection.ConnectionState.Connected;
            connection.EndPoint = endPoint;

            return true;
        }

        private uint NewAcknowledgementKey()
        {
            return RandomHelper.NextUInt();
        }
    }
}
