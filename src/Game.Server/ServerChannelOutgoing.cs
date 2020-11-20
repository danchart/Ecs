using Common.Core;
using Game.Networking;
using Game.Simulation.Server;
using System;
using System.Net;

namespace Game.Server
{
    /// <summary>
    /// Handles queueing replication packets to clients in priority order.
    /// </summary>
    public sealed class ServerChannelOutgoing
    {
        private ServerPacketEnvelope _serverPacketEnvelope;

        private readonly IPacketEncryptor _packetEncryption;
        private readonly ServerUdpPacketTransport _transport;
        private readonly NetworkTransportConfig _config;
        private readonly ILogger _logger;

        public ServerChannelOutgoing(
            NetworkTransportConfig config,
            ServerUdpPacketTransport transport,
            IPacketEncryptor packetEncryption,
            ILogger logger)
        {
            this._config = config ?? throw new ArgumentNullException(nameof(config));
            this._transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this._packetEncryption = packetEncryption ?? throw new ArgumentNullException(nameof(packetEncryption));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void SendClientPacket(IPEndPoint endPoint, in ServerPacketEnvelope serverPacket)
        {
            // TODO: Add flow control?
            this._transport.SendPacket(endPoint, in serverPacket);
        }

        // TODO:
        // 1) Queue replication packets to transport
        // 2) Sort client input to worlds 

        public void ReplicateToClients(ushort frame, WorldPlayers players)
        {
            // TODO: Do we need a packet pool? A singleton works single threaded only.
            ref var packet = ref this._serverPacketEnvelope;

            packet.Type = ServerPacketType.Replication;

            foreach (ref var player in players)
            {
                ref readonly var playerConnection = ref player.ConnectionRef.Unref();

                packet.PlayerId = playerConnection.PlayerId;
                packet.SimulationPacket.Frame = frame;
                packet.SimulationPacket.EntityCount = 0;

                if (packet.SimulationPacket.Entities == null)
                {
                    packet.SimulationPacket.Entities = new EntityPacketData[64];
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

                            if (packet.SimulationPacket.Entities[packet.SimulationPacket.EntityCount].Components == null)
                            {
                                packet.SimulationPacket.Entities[packet.SimulationPacket.EntityCount].Components = new ComponentPacketData[128];
                            }

                            packet.SimulationPacket.Entities[packet.SimulationPacket.EntityCount].ItemCount = 0;

                            replicatedEntity.ToEntityComponentPackets(
                                ref packet.SimulationPacket.Entities[packet.SimulationPacket.EntityCount].Components,
                                ref packet.SimulationPacket.Entities[packet.SimulationPacket.EntityCount].ItemCount);
                        }
                    }
                }

                this._transport.SendPacket(playerConnection.EndPoint, in packet);
            }
        }
    }
}
