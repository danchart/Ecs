using Common.Core;
using Game.Networking;
using System;
using System.Net;

namespace Game.Simulation.Server
{
    public sealed class PlayerConnectionManager
    {
        internal readonly RefDictionary<PlayerId, PlayerConnection> _connections;

        private readonly ILogger _logger;
        
        public PlayerConnectionManager(ILogger logger, PlayerConnectionConfig playerConnectionConfig)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._connections = new RefDictionary<PlayerId, PlayerConnection>(playerConnectionConfig.Capacity.InitialConnectionsCapacity);
        }

        public int Count
        {
            get => this._connections.Count;
        }

        public ref PlayerConnection this[PlayerId playerId]
        {
            get => ref this._connections[playerId];
        }

        public bool HasPlayer(PlayerId playerId) => this._connections.ContainsKey(playerId);

        public PlayerConnectionRef GetRef(PlayerId id)
        {
            return new PlayerConnectionRef(id, this);
        }

        public void Add(
            WorldInstanceId worldInstanceId, 
            PlayerId playerId, 
            byte[] encryptionKey)
        {
            this._connections.Add(playerId);

            ref var connection = ref this._connections[playerId];

            connection.State = PlayerConnection.ConnectionState.PreConnected;
            connection.WorldInstanceId = worldInstanceId;
            connection.PlayerId = playerId;
            connection.PacketEncryptionKey = encryptionKey;
        }

        public void Remove(PlayerId playerId)
        {
            this._connections.Remove(playerId);
        }

        public RefDictionary<PlayerId, PlayerConnection>.Enumerator GetEnumerator()
        {
            return this._connections.GetEnumerator();
        }
    }
}
