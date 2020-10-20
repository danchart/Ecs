using Game.Networking;

namespace Game.Simulation.Server
{
    /// <summary>
    /// Reference to player connection struct.
    /// </summary>
    public struct PlayerConnectionRef
    {
        private readonly PlayerId _id;
        private readonly PlayerConnectionManager _connections;

        internal PlayerConnectionRef(PlayerId id, in PlayerConnectionManager connections)
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
