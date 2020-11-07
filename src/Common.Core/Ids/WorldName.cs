using System;

namespace Common.Core
{
    public readonly struct WorldName : IEquatable<WorldName>
    {
        internal readonly int Id;

        public WorldName(int id)
        {
            Id = id;
        }

        public static implicit operator int(WorldName id) => id.Id;

        public static bool operator ==(in WorldName lhs, in WorldName rhs)
        {
            return
                lhs.Id == rhs.Id;
        }

        public static bool operator !=(in WorldName lhs, in WorldName rhs)
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
                other is WorldName otherEntity &&
                Equals(otherEntity);
        }

        public bool Equals(WorldName entity)
        {
            return
                Id == entity.Id;
        }
    }
}
