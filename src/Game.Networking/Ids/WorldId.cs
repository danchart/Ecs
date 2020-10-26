using System;

namespace Game.Networking
{
    public readonly struct WorldId : IEquatable<WorldId>
    {
        internal readonly int Id;

        public WorldId(int id)
        {
            Id = id;
        }

        public static implicit operator int(WorldId id) => id.Id;

        public static bool operator ==(in WorldId lhs, in WorldId rhs)
        {
            return
                lhs.Id == rhs.Id;
        }

        public static bool operator !=(in WorldId lhs, in WorldId rhs)
        {
            return
                lhs.Id != rhs.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override bool Equals(object other)
        {
            return
                other is WorldId otherEntity &&
                Equals(otherEntity);
        }

        public bool Equals(WorldId entity)
        {
            return
                Id == entity.Id;
        }
    }
}
