using Common.Core;
using Ecs.Core;

namespace Game.Simulation.Server
{
    public class PlayerConnections
    {
        private readonly RefDictionary<int, PlayerConnection> _connections;

        private readonly PlayerConnectionConfig _playerConnectionConfig;
        private readonly ReplicationConfig _replicationConfig;

        public PlayerConnections(ReplicationConfig replicationConfig, PlayerConnectionConfig playerConnectionConfig)
        {
            this._playerConnectionConfig = playerConnectionConfig;
            this._replicationConfig = replicationConfig;
            this._connections = new RefDictionary<int, PlayerConnection>(playerConnectionConfig.Capacity.InitialConnectionsCapacity);
        }

        public int Count
        {
            get => this._connections.Count;
        }

        public ref PlayerConnection this[int playerId]
        {
            get => ref this._connections[playerId];
        }

        public bool HasPlayer(int playerId) => this._connections.ContainsKey(playerId);

        public void Add(int playerId, in Entity entity)
        {
            this._connections.Add(playerId);

            ref var connection = ref this._connections[playerId];

            connection.PlayerId = playerId;
            connection.Entity = entity;

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

        public void Remove(int playerId)
        {
            this._connections.Remove(playerId);
        }
    }
}
