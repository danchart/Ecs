using Game.Simulation.Server;
using System;
using System.Collections.Generic;

namespace Game.Server
{
    public sealed class ServerChannelManager
    {
        private ServerUdpPacketTransport _transport;
        private Dictionary<int, WorldPlayers> _worldPlayers;

        public ServerChannelManager(ServerUdpPacketTransport transport)
        {
            this._transport = transport ?? throw new ArgumentNullException(nameof(transport));

            this._worldPlayers = new Dictionary<int, WorldPlayers>(8);
        }

        public void AddWorld(int worldId, WorldPlayers worldPlayers)
        {
            this._worldPlayers[worldId] = worldPlayers;
        }

        public void RemoveWorld(int worldId)
        {
            this._worldPlayers.Remove(worldId);
        }


    }
}
