using Common.Core;
using Ecs.Core;
using Game.Networking;

namespace Game.Simulation.Server
{
    public class PlayerConnectionManager
    {
        internal readonly RefDictionary<PlayerId, PlayerConnection> _connections;

        private readonly PlayerConnectionConfig _playerConnectionConfig;
        private readonly ReplicationConfig _replicationConfig;

        public PlayerConnectionManager(ReplicationConfig replicationConfig, PlayerConnectionConfig playerConnectionConfig)
        {
            this._playerConnectionConfig = playerConnectionConfig;
            this._replicationConfig = replicationConfig;
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

        public void Add(PlayerId playerId, in Entity entity, byte[] encryptionKey)
        {
            this._connections.Add(playerId);

            ref var connection = ref this._connections[playerId];

            connection.PlayerId = playerId;
            connection.Entity = entity;
            connection.PacketEncryptionKey = encryptionKey;

            if (connection.ReplicationData == null)
            {
                connection.ReplicationData = new PlayerReplicationData(
                    this._replicationConfig.Capacity.InitialReplicatedEntityCapacity,
                    this._replicationConfig.Networking.PriorityQueueDelayBaseTick,
                    this._replicationConfig.Networking.PriorityQueueDelay);
            }
            else
            {
                connection.ReplicationData.Clear();
            }
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
