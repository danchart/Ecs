using Ecs.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Game.Simulation.Server
{
    public class PlayerConnections
    {
        private PlayerConnection[] _connections;
        private int[] _freeIndices;

        private int _count;
        private int _freeCount;

        private Dictionary<int, int> _playerIdToIndex;

        public PlayerConnections(int capacity)
        {
            this._connections = new PlayerConnection[capacity];
            this._freeIndices = new int[capacity];
            this._playerIdToIndex = new Dictionary<int, int>(capacity);

            this._count = 0;
            this._freeCount = 0;
        }

        public int Count
        {
            get => this._count;
        }

        public ref PlayerConnection this[int playerId]
        {
            get => ref this._connections[_playerIdToIndex[playerId]];
        }

        public void Add(int playerId, in Entity entity)
        {
            Debug.Assert(!this._playerIdToIndex.ContainsKey(playerId));

            int index;

            if (this._freeCount > 0)
            {
                index = this._freeIndices[--this._freeCount];
            }
            else
            {
                if (this._count == this._connections.Length)
                {
                    Array.Resize(ref _connections, 2 * this._count);
                    Array.Resize(ref _freeIndices, 2 * this._count);
                }

                index = this._count++;
            }

            this._playerIdToIndex[playerId] = index;
            this._connections[index].Assign(playerId, entity);
        }

        public void Remove(int playerId)
        {
            this._count--;

            this._freeIndices[_freeCount++] = this._playerIdToIndex[playerId];
            this._playerIdToIndex.Remove(playerId);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        private static void Assign(
            ref PlayerConnection connection,
            int playerId,
            in Entity entity)
        {
            connection.PlayerId = playerId;
            connection.Entity = entity;

            if (connection.ReplicationData == null)
            {
                connection.ReplicationData = new PlayerReplicationData()
            }

            connection.ReplicationData.Clear();
        }


        public struct Enumerator
        {
            private readonly PlayerConnections _parent;

            private readonly Dictionary<int, int>.Enumerator _enumerator;

            internal Enumerator(PlayerConnections parent)
            {
                this._parent = parent;

                this._enumerator = parent._playerIdToIndex.GetEnumerator();
            }

            public ref PlayerConnection Current
            {
                get => ref this._parent._connections[this._enumerator.Current.Value];
            }

            public bool MoveNext()
            {
                return this._enumerator.MoveNext();
            }
        }
    }
}
