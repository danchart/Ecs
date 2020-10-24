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
            RefDictionary<int, PlayerReplicationData.EntityReplicationData> playerReplicatedEntities = new RefDictionary<int, PlayerReplicationData.EntityReplicationData>(16);
            AppendOnlyList< PlayerReplicationData.EntityReplicationData >

            foreach (var pair in this._worldPlayersMap)
            {
                var worldPlayers = pair.Value;

                foreach (ref var player in worldPlayers)
                {
                    playerReplicatedEntities.Clear();

                    ref readonly var playerConnection = ref player.ConnectionRef.Unref();

                    ServerPacket packet;

                    packet.Type = ServerPacketType.Simulation;
                    packet.PlayerId = playerConnection.PlayerId;
                    packet.SimulationPacket.Frame = frame;
                    //packet.SimulationPacket.EntityCount =
                    //packet.SimulationPacket.EntityData =

                    // HACK: This is SUPER fragile!!
                    const int ServerPacketHeaderSize = 16; 

                    int size = 0;

                    foreach (var replicatedEntity in player.ReplicationData)
                    {
                        if (replicatedEntity.NetPriority.RemainingQueueTime <= 0)
                        {
                            var entitySize = replicatedEntity.MeasurePacketSize(playerConnection.PacketEncryptionKey);

                            if (size + entitySize > this._config.MaxPacketSize - ServerPacketHeaderSize)
                            {
                                // Reached packet size
                                break;
                            }
                            else
                            {
                                size += entitySize;

                                playerReplicatedEntities.Add(playerReplicatedEntities.Count, replicatedEntity);
                            }
                        }
                    }
                }
            }
        }
    }
}
