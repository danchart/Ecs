using Game.Simulation.Server;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Game.Server
{
    public sealed class ServerChannelManager
    {
        private ServerUdpPacketTransport _transport;
        private Dictionary<int, WorldPlayers> _worldPlayersMap;

        public ServerChannelManager(ServerUdpPacketTransport transport)
        {
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

        // TODO: Queue replication packets to transport
        // TODO: Sort client input to worlds

        public void UpdateClients()
        {
            foreach (var pair in this._worldPlayersMap)
            {
                var worldPlayers = pair.Value;

                foreach (ref var player in worldPlayers)
                {
                    player.ReplicationData.
                }
            }
        }
    }
}
