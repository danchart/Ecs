using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Game.Simulation.Server
{
    public interface IPlayerConnectionManager
    {
        //Dictionary<int, PlayerConnection> Connections { get; }
    }

    public class PlayerConnectionManager : IPlayerConnectionManager
    {
        private PlayerConnection[] _connections;
        private int[] _freeIndices;

        private int _count;
        private int _freeCount;

        private Dictionary<int, int> _playerIdToIndex;

        public PlayerConnectionManager(int capacity)
        {
            this._connections = new PlayerConnection[capacity];
            this._freeIndices = new int[capacity];
            this._playerIdToIndex = new Dictionary<int, int>(capacity);

            this._count = 0;
            this._freeCount = 0;
        }

        public void Add(int playerId)
        {
            Debug.Assert(!this._playerIdToIndex.ContainsKey(playerId));

            if (this._count == this._connections.Length)
            {
                Array.Resize(ref _connections, 2 * this._count);
                Array.Resize(ref _freeIndices, 2 * this._count);
            }

            if (_freeCount > 0)
            {
                var index = _freeIndices[--_freeCount];

                this._playerIdToIndex[playerId] = index;

                _connections[index].Clear();
            }

            
        }

        ///public Dictionary<int, PlayerConnection> Connections { get; } = new Dictionary<int, PlayerConnection>();
    }
}
