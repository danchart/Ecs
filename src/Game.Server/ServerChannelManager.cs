using Common.Core;
using Game.Networking;
using Game.Networking.Packets;
using Game.Simulation.Server;
using System;
using System.IO;
using System.Threading;

namespace Game.Server
{
    /// <summary>
    /// Handles queueing replication packets to clients in priority order.
    /// </summary>
    public sealed class ServerChannelManager
    {
        private ServerPacketEnvelope _serverPacketEnvelope;
        private ClientPacketEnvelope _clientPacketEnvelope;

        private bool _isStopRequested;

        private readonly IPacketEncryption _packetEncryption;
        private readonly ServerUdpPacketTransport _transport;
        private readonly TransportConfig _config;
        private readonly ILogger _logger;

        public ServerChannelManager(
            TransportConfig config, 
            ServerUdpPacketTransport transport,
            IPacketEncryption packetEncryption,
            ILogger logger)
        {
            this._isStopRequested = false;

            this._config = config ?? throw new ArgumentNullException(nameof(config));
            this._transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this._packetEncryption = packetEncryption ?? throw new ArgumentNullException(nameof(packetEncryption));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Start()
        {
            var thread = new Thread(ProcessReceiveBuffer);
            thread.Start();
        }


        public void Stop()
        {
            this._isStopRequested = true;
        }

        private void ProcessReceiveBuffer()
        {
            byte[] data;

            while (!this._isStopRequested)
            {
                var packetCount = this._transport.ReceiveBuffer.Count;

                while (packetCount-- > 0)
                {
                    int offset, count;
                    if (this._transport.ReceiveBuffer.GetReadData(out data, out offset, out count))
                    {
                        using (var stream = new MemoryStream(data, offset, count))
                        {
                            if (!_clientPacketEnvelope.Deserialize(stream, this._packetEncryption))
                            {
                                this._logger.Verbose("Failed to deserialize packet.");
                            }

                            // Process player packet / input

                            switch (_clientPacketEnvelope.Type)
                            {
                                case ClientPacketType.Control:

                                    ProcessClientControlPacket(_clientPacketEnvelope.PlayerId, _clientPacketEnvelope.ControlPacket);
                                    break;
                                case ClientPacketType.PlayerInput:

                                    ProcessPlayerInputPacket(_clientPacketEnvelope.PlayerId, _clientPacketEnvelope.PlayerInputPacket);
                                    break;
                                default:

                                    this._logger.Error($"Unknown packet type {_clientPacketEnvelope.Type}");
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void ProcessClientControlPacket(PlayerId playerId, ControlPacket controlPacket)
        {
            switch (controlPacket.ControlMessage)
            {
                case ControlMessageEnum.ConnectSyn:



                    break;

                case ControlMessageEnum.ConnectAck:

                    break;
            }
        }

        private void ProcessPlayerInputPacket(PlayerId playerId, ClientPlayerInputPacket playerInputPacket)
        {

        }

        // TODO:
        // 1) Queue replication packets to transport
        // 2) Sort client input to worlds 

        public void SendWorldUpdateToClients(ushort frame, WorldPlayers players)
        {
            // TODO: Do we need a packet pool? A singleton works single threaded only.
            ref var packet = ref this._serverPacketEnvelope;

            packet.Type = ServerPacketType.Simulation;

            foreach (ref var player in players)
            {
                ref readonly var playerConnection = ref player.ConnectionRef.Unref();

                packet.PlayerId = playerConnection.PlayerId;
                packet.SimulationPacket.Frame = frame;
                packet.SimulationPacket.EntityCount = 0;

                if (packet.SimulationPacket.EntityData == null)
                {
                    packet.SimulationPacket.EntityData = new EntityPacketData[64];
                }

                // HACK: This is SUPER fragile!!
                const int ServerPacketHeaderSize = 16;

                int size = 0;

                foreach (var replicatedEntity in player.ReplicationData)
                {
                    if (replicatedEntity.NetPriority.RemainingQueueTime <= 0)
                    {
                        var entitySize = replicatedEntity.MeasurePacketSize();

                        if (size + entitySize > this._config.MaxPacketSize - ServerPacketHeaderSize)
                        {
                            // Reached packet size 
                            break;
                        }
                        else
                        {
                            packet.SimulationPacket.EntityCount++;
                            size += entitySize;

                            if (packet.SimulationPacket.EntityData[packet.SimulationPacket.EntityCount].Components == null)
                            {
                                packet.SimulationPacket.EntityData[packet.SimulationPacket.EntityCount].Components = new ComponentPacketData[128];
                            }

                            packet.SimulationPacket.EntityData[packet.SimulationPacket.EntityCount].ItemCount = 0;

                            replicatedEntity.ToEntityComponentPackets(
                                ref packet.SimulationPacket.EntityData[packet.SimulationPacket.EntityCount].Components,
                                ref packet.SimulationPacket.EntityData[packet.SimulationPacket.EntityCount].ItemCount);
                        }
                    }
                }

                this._transport.SendPacket(playerConnection.EndPoint, in packet);
            }
        }
    }
}
