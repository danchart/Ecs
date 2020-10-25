using Game.Networking;
using System;
using System.Collections.Generic;

namespace Game.Networking
{
    public sealed class PlayerConnectionRefs
    {
        private readonly Dictionary<PlayerId, PlayerConnectionRef> _connectionRefs = new Dictionary<PlayerId, PlayerConnectionRef>();
        private readonly PlayerConnectionManager _connectionManager;

        public PlayerConnectionRefs(PlayerConnectionManager connections)
        {
            this._connectionManager = connections ?? throw new ArgumentNullException(nameof(connections));
        }

        public bool HasPlayer(PlayerId id)
        {
            return this._connectionRefs.ContainsKey(id);
        }

        public void Add(PlayerId id)
        {
            this._connectionRefs[id] = this._connectionManager.GetRef(id);
        }

        public void Remove(PlayerId id)
        {
            this._connectionRefs.Remove(id);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this._connectionRefs);
        }

        public struct Enumerator
        {
            private Dictionary<PlayerId, PlayerConnectionRef>.Enumerator _enumerator;

            internal Enumerator(Dictionary<PlayerId, PlayerConnectionRef> dictionary)
            {
                this._enumerator = dictionary.GetEnumerator();
            }

            public PlayerConnectionRef Current
            {
                get => this._enumerator.Current.Value;
            }

            public bool MoveNext() => this._enumerator.MoveNext();
        }
    }
}
