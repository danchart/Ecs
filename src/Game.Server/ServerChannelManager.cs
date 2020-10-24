using Common.Core;
using Ecs.Core;
using Game.Networking;
using Game.Simulation.Core;
using Game.Simulation.Server;
using System;
using System.Collections.Generic;

namespace Game.Server
{
    public sealed class ServerChannelManager
    {
        private ServerUdpPacketTransport _transport;
        private Dictionary<int, WorldPlayers> _worldPlayersMap;

        private readonly TransportConfig _config;

        private ServerPacket _packet;

        public ServerChannelManager(TransportConfig config, ServerUdpPacketTransport transport)
        {
            this._config = config ?? throw new ArgumentNullException(nameof(config));
            this._transport = transport ?? throw new ArgumentNullException(nameof(transport));

            this._worldPlayersMap = new Dictionary<int, WorldPlayers>(8);
        }

        public void AddWorld(int worldId, WorldPlayers worldPlayers)
        {
            this._worldPlayersMap[worldId] = worldPlayers;
        }

        public void RemoveWorld(int worldId)
        {
            this._worldPlayersMap.Remove(worldId);
        }

        // TODO:
        // 1) Queue replication packets to transport
        // 2) Sort client input to worlds

        public void UpdateClients(ushort frame)
        {
            // TODO: Do we need a packet pool?
            ref var packet = ref this._packet;

            packet.Type = ServerPacketType.Simulation;

            foreach (var pair in this._worldPlayersMap)
            {
                var worldPlayers = pair.Value;

                foreach (ref var player in worldPlayers)
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

                                if (packet.SimulationPacket.EntityData[packet.SimulationPacket.EntityCount].Items == null)
                                {
                                    packet.SimulationPacket.EntityData[packet.SimulationPacket.EntityCount].Items = new PacketDataItem[128];
                                }

                                packet.SimulationPacket.EntityData[packet.SimulationPacket.EntityCount].ItemCount = 0;

                                // TODO: Move this to  PacketDataItem
                                replicatedEntity.ToPacketDataItems(
                                    ref packet.SimulationPacket.EntityData[packet.SimulationPacket.EntityCount].Items, 
                                    ref packet.SimulationPacket.EntityData[packet.SimulationPacket.EntityCount].ItemCount);
                            }
                        }
                    }
                }
            }
        }
    }
}
