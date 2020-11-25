using Common.Core;
using Game.Networking;
using System;

namespace Game.Client
{
    public class ClientSimulationController
    {
        private readonly GameServerConnection _connection;
        private readonly ClientUdpPacketTransport _transport;
        private readonly PacketJitterBuffer _jitterBuffer;
        private readonly ILogger _logger;

        public ClientSimulationController(
            ILogger logger, 
            GameServerConnection connection, 
            ClientUdpPacketTransport transport,
            PacketJitterBuffer jitterBuffer)
        {
            this._connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this._transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this._jitterBuffer = jitterBuffer ?? throw new ArgumentNullException(nameof(jitterBuffer));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool Process(in ReplicationPacket packet)
        {
            // Add packet to the jitter buffer
            return this._jitterBuffer.AddPacket(packet);
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
            packet.ControlPacket.ControlMessage = ControlMessageEnum.ConnectAck;
            packet.ControlPacket.ControlAckPacketData.SequenceKey = this._connection.Handshake.SequenceKey;
            packet.ControlPacket.ControlAckPacketData.AcknowledgementKey = this._connection.Handshake.AcknowledgementKey;

            this._transport.SendPacket(packet);

            this._logger.Info($"Sent ACK handshake: sequenceKey={this._connection.Handshake.SequenceKey}, ackKey={this._connection.Handshake.AcknowledgementKey}");

            return true;
        }
    }
}
