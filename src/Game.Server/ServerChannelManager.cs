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
        private ServerPacket _serverPacket;
        private ClientPacket _clientPacket;

        private bool _isStopRequested;

        private readonly IPacketEncryption _packetEncryption;
        private readonly ServerUdpPacketTransport _transport;
        private readonly TransportConfig _config;

        public ServerChannelManager(
            TransportConfig config, 
            ServerUdpPacketTransport transport,
            IPacketEncryption packetEncryption)
        {
            this._isStopRequested = false;

            this._config = config ?? throw new ArgumentNullException(nameof(config));
            this._transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this._packetEncryption = packetEncryption ?? throw new ArgumentNullException(nameof(packetEncryption));
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
                            _clientPacket.Deserialize(stream, this._packetEncryption);

                            _clientPacket.PlayerId
                        }
                    }
                }
            }
        }

        // TODO:
        // 1) Queue replication packets to transport
        // 2) Sort client input to worlds 

        public void SendWorldUpdateToClients(ushort frame, WorldPlayers players)
        {
            // TODO: Do we need a packet pool?
            ref var packet = ref this._serverPacket;

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

                            // TODO: Move this to  PacketDataItem
                            replicatedEntity.ToPacketDataItems(
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
