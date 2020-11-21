using Common.Core;
using Game.Networking;
using System;
using System.Net;

namespace Game.Client
{
    public class ControlPlaneClientController
    {
        private readonly ILogger _logger;

        public ControlPlaneClientController(ILogger logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool Process(in ControlPacket controlPacket)
        {
            switch (controlPacket.ControlMessage)
            {
                case ControlMessageEnum.ConnectSynAck:

                    return SynAckHandshake(controlPacket.ControlAckPacketData.SequenceKey);
            }

            return false;
        }

        public bool SynAckHandshake(uint sequenceKey)
        {

            ref var connection = ref this._playerConnections[playerId];

            if (connection.State == PlayerConnection.ConnectionState.Connected)
            {
                this._logger.VerboseError($"Client SYN request for connected player: Id={playerId}");

                return false;
            }

            // Send SYN-ACK

            connection.Handshake.AcknowledgementKey = NewAcknowledgementKey();
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

        private uint NewAcknowledgementKey()
        {
            return RandomHelper.NextUInt();
        }
    }
}
