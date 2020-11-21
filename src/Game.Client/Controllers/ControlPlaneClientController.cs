﻿using Common.Core;
using Game.Networking;
using System;
using System.Net;

namespace Game.Client
{
    public class ControlPlaneClientController
    {
        private readonly GameServerConnection _connection;
        private readonly ClientUdpPacketTransport _transport;
        private readonly ILogger _logger;

        public ControlPlaneClientController(ILogger logger, GameServerConnection connection, ClientUdpPacketTransport transport)
        {
            this._connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this._transport = transport ?? throw new ArgumentNullException(nameof(transport));
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

        public bool SynAckHandshake(uint acknowledgementKey)
        {
            if (this._connection.State == GameServerConnection.ConnectionState.Connected)
            {
                this._logger.VerboseError($"SYN-ACK request for connected player.");

                return false;
            }

            // Send ACK

            this._connection.Handshake.AcknowledgementKey = acknowledgementKey;
            this._connection.State = GameServerConnection.ConnectionState.Connected;

            var packet = new ClientPacketEnvelope();

            packet.Type = ClientPacketType.ControlPlane;
            packet.PlayerId = this._connection.PlayerId;
            packet.ControlPacket.ControlAckPacketData.SequenceKey = this._connection.Handshake.SequenceKey;
            packet.ControlPacket.ControlAckPacketData.AcknowledgementKey = this._connection.Handshake.AcknowledgementKey;

            this._transport.SendPacket(packet);

            return true;
        }
    }
}
