using Game.Networking;

namespace Game.Simulation.Server
{
    public struct PlayerConnectionRef
    {
        private readonly PlayerId _id;
        private readonly PlayerConnections _connections;

        internal PlayerConnectionRef(PlayerId id, in PlayerConnections connections)
        {
            this._id = id;
            this._connections = connections;
        }

        public ref PlayerConnection Unref()
        {
            return ref this._connections[this._id];
        }

        public static bool operator ==(in PlayerConnectionRef lhs, in PlayerConnectionRef rhs)
        {
            return lhs._id == rhs._id;
        }

        public static bool operator !=(in PlayerConnectionRef lhs, in PlayerConnectionRef rhs)
        {
            return lhs._id != rhs._id;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerConnectionRef other && Equals(other);
        }

        public override int GetHashCode()
        {
            return this._id.GetHashCode();
        }
    }
}
