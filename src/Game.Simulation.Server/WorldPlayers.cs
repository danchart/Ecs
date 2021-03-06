﻿using Common.Core;
using Game.Simulation.Core;
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
            PlayerInputConfig playerInputConfig,
            int capacity)
        {
            this._replicationDataPool = new PlayerReplicationDataPool(replicationConfig, capacity);
            this._playerInputsPool = new PlayerInputsPool(
                inputFrameCount: playerInputConfig.MaxFrameCount,
                capacity: capacity);
            this._playerIdToIndex = new Dictionary<PlayerId, int>(capacity);
            this._players = new WorldPlayer[capacity];
            this._count = 0;
        }

        public bool Contains(in PlayerId id) => this._playerIdToIndex.ContainsKey(id);

        public ref WorldPlayer this[in PlayerId id] => ref this._players[this._playerIdToIndex[id]];

        public void Add(in PlayerConnectionRef playerConnectionRef)
        {
            if (this._count == this._players.Length)
            {
                Array.Resize(ref this._players, 2 * this._count);
            }

            var index = this._count++;

            this._players[index] = new WorldPlayer(this._replicationDataPool, _playerInputsPool, playerConnectionRef);

            var playerId = playerConnectionRef.Unref().PlayerId;

            this._playerIdToIndex[playerId] = index;
        }

        public void Remove(in PlayerId playerId)
        {
            var indexToRemove = this._playerIdToIndex[playerId];

            this._players[indexToRemove].Free();
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
            private readonly WorldPlayer[] _players;
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
