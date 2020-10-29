using Common.Core;
using Ecs.Core;
using Game.Networking;
using System;
using System.Collections.Generic;

namespace Game.Simulation.Server
{
    public sealed class WorldPlayers
    {
        private WorldPlayer[] _players;
        private int _count;

        private readonly Dictionary<PlayerId, int> _playerIdToIndex;
        private readonly PlayerReplicationDataPool _replicationDataPool;
        private readonly PlayerInputsPool _playerInputsPool;

        public WorldPlayers(
            ReplicationConfig replicationConfig,
            WorldConfig worldConfig,
            int capacity)
        {
            this._replicationDataPool = new PlayerReplicationDataPool(replicationConfig, capacity);
            this._playerInputsPool = new PlayerInputsPool(
                inputFrameCount: worldConfig.PlayerInputMaxFrameCount,
                capacity: capacity);
            this._playerIdToIndex = new Dictionary<PlayerId, int>(capacity);
            this._players = new WorldPlayer[capacity];
            this._count = 0;
        }

        public bool Contains(PlayerId id) => this._playerIdToIndex.ContainsKey(id);

        public ref WorldPlayer this[PlayerId id] => ref this._players[this._playerIdToIndex[id]];

        public void Add(
            in PlayerConnectionRef playerConnectionRef,
            in Entity entity)
        {
            if (this._count == this._players.Length)
            {
                Array.Resize(ref this._players, 2 * this._count);
            }

            var index = this._count++;

            ref var player = ref this._players[index];

            player.ConnectionRef = playerConnectionRef;
            player.Entity = entity;
            player.PlayerReplicationDataPoolIndex = this._replicationDataPool.New();
            player.PlayerInputsPoolIndex = this._playerInputsPool.New();

            var playerId = playerConnectionRef.Unref().PlayerId;

            this._playerIdToIndex[playerId] = index;
        }

        public void Remove(PlayerId playerId)
        {
            var indexToRemove = this._playerIdToIndex[playerId];

            this._replicationDataPool.Free(this._players[indexToRemove].PlayerReplicationDataPoolIndex);
            this._playerInputsPool.Free(this._players[indexToRemove].PlayerInputsPoolIndex);
            this._playerIdToIndex.Remove(playerId);

            // Swap index with last if at least one element remains.
            if (--this._count > 0)
            {
                this._players[indexToRemove] = this._players[this._count];
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

            public bool MoveNext() => ++this._current < this._count;
        }
    }
}
