using Ecs.Core;
using Game.Networking;
using System;
using System.Collections.Generic;

namespace Game.Simulation.Server
{
    public class WorldPlayers
    {
        private WorldPlayer[] _players;

        private Dictionary<PlayerId, int> _playerIdToIndex;

        private int _count;

        private readonly PlayerReplicationDataPool _replicationDataPool;
        private readonly PlayerConnectionManager _connectionManager;

        public WorldPlayers(
            PlayerConnectionManager connectionManager,
            ReplicationConfig replicationConfig,
            int capacity)
        {
            this._connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            this._replicationDataPool = new PlayerReplicationDataPool(replicationConfig, capacity);
            this._playerIdToIndex = new Dictionary<PlayerId, int>(capacity);
            this._players = new WorldPlayer[capacity];
            this._count = 0;
        }

        public ref WorldPlayer GetItem(PlayerId id) => ref this._players[this._playerIdToIndex[id]];

        public void Add(
            PlayerId playerId,
            in Entity entity)
        {
            if (this._count == this._players.Length)
            {
                Array.Resize(ref this._players, 2 * this._count);
            }

            var index = this._count++;

            ref var player = ref this._players[index];

            player.ConnectionRef = this._connectionManager.GetRef(playerId);
            player.Entity = entity;
            player.PlayerReplicationDataIndex = this._replicationDataPool.New();

            this._playerIdToIndex[playerId] = index;
        }

        public void Remove(PlayerId playerId)
        {
            var index = this._playerIdToIndex[playerId];

            this._replicationDataPool.Free(this._players[index].PlayerReplicationDataIndex);
            this._playerIdToIndex.Remove(playerId);

            // Swap index with last if at least one element remains.
            if (--this._count > 0)
            {
                this._players[index] = this._players[this._count];
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this._players, this._count);
        }

        public struct Enumerator
        {
            private WorldPlayer[] _players;
            private int _count;

            private int _current;

            public Enumerator(WorldPlayer[] players, int count)
            {
                this._players = players ?? throw new ArgumentNullException(nameof(players));
                this._count = count;

                this._current = -1;
            }

            public ref WorldPlayer Current 
            {
                get => ref this._players[this._current];
            }

            public bool MoveNext() => 
                ++this._current < this._count 
                ? true 
                : false;
        }

        public struct WorldPlayer
        {
            public PlayerConnectionRef ConnectionRef;

            public Entity Entity;

            public int PlayerReplicationDataIndex;

            private readonly PlayerReplicationDataPool _replicationDataPool;

            internal WorldPlayer(PlayerReplicationDataPool replicationDataPool) : this()
            {
                this._replicationDataPool = replicationDataPool ?? throw new ArgumentNullException(nameof(replicationDataPool));
            }

            public PlayerReplicationData ReplicationData
            {
                get => this._replicationDataPool.GetItem(PlayerReplicationDataIndex);
            }
        }
    }
}
