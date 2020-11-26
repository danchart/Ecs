using Common.Core;
using Ecs.Core;
using Game.Networking;
using Game.Simulation.Server;
using System;
using System.IO;
using System.Net;

namespace Game.Server
{
    /// <summary>
    /// Handles queueing replication packets to clients in priority order.
    /// </summary>
    public sealed class OutgoingServerChannel
    {
        private readonly IPacketEncryptor _packetEncryption;
        private readonly ServerUdpPacketTransport _transport;
        private readonly NetworkTransportConfig _config;
        private readonly ILogger _logger;

        private readonly AppendOnlyList<Entity> _clientEntitiesToRemove;

        public OutgoingServerChannel(
            NetworkTransportConfig config,
            ServerUdpPacketTransport transport,
            IPacketEncryptor packetEncryption,
            ILogger logger)
        {
            this._config = config ?? throw new ArgumentNullException(nameof(config));
            this._transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this._packetEncryption = packetEncryption ?? throw new ArgumentNullException(nameof(packetEncryption));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this._clientEntitiesToRemove = new AppendOnlyList<Entity>(64);
        }

        public void SendClientPacket(IPEndPoint endPoint, in ServerPacketEnvelope serverPacket)
        {
            // TODO: Add flow control?
            this._transport.SendPacket(endPoint, in serverPacket);
        }

        // TODO:
        // 1) Queue replication packets to transport
        // 2) Sort client input to worlds 

        public void ReplicateToClients(
            float deltaTime,
            WorldPlayers players)
        {
            ServerPacketEnvelope packet = default;

            packet.Type = ServerPacketType.Replication;

            foreach (ref var player in players)
            {
                ref readonly var playerConnection = ref player.ConnectionRef.Unref();

                if (playerConnection.State != PlayerConnection.ConnectionState.Connected)
                {
                    continue;
                }

                playerConnection.Frame = playerConnection.Frame + 1;

                packet.PlayerId = playerConnection.PlayerId;
                packet.ReplicationPacket.FrameNumber = playerConnection.Frame;
                packet.ReplicationPacket.EntityCount = 0;

                if (packet.ReplicationPacket.Entities == null)
                {
                    packet.ReplicationPacket.Entities = new EntityPacketData[64];
                }

                int size = ServerPacketEnvelope.EnvelopeSize;

                this._clientEntitiesToRemove.Clear();

                foreach (int index in player.ReplicationData)
                {
                    ref var replicatedEntity = ref player.ReplicationData[index];

                    if (replicatedEntity.NetPriority.RemainingQueueTime <= 0)
                    {
                        ref var entityPacketData = ref packet.ReplicationPacket.Entities[packet.ReplicationPacket.EntityCount];

                        replicatedEntity.ToEntityPacketData(ref entityPacketData);

                        var entitySize = entityPacketData.Serialize(Stream.Null, measureOnly: true);

                        if (size + entitySize > this._config.MaxPacketSize)
                        {
                            // Packet size limit reached.
                            break;
                        }

                        size += entitySize;
                        packet.ReplicationPacket.EntityCount++;

                        this._clientEntitiesToRemove.Add(replicatedEntity.Entity);
                    }
                    else
                    {
                        replicatedEntity.NetPriority.RemainingQueueTime -= deltaTime;
                    }
                }

                // TODO: Instead of removing we must mark these as pending-sent until we get ack from
                // client for this frame number.
                for (int i = 0; i < this._clientEntitiesToRemove.Count; i++)
                {
                    player.ReplicationData.Remove(_clientEntitiesToRemove.Items[i]);
                }

                this._transport.SendPacket(playerConnection.EndPoint, in packet);
            }
        }
    }
}
