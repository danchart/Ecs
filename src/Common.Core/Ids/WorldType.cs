using System;

namespace Common.Core
{
    public readonly struct WorldType : IEquatable<WorldType>
    {
        internal readonly int Id;

        public WorldType(int id)
        {
            Id = id;
        }

        public static implicit operator int(WorldType id) => id.Id;

        public static bool operator ==(in WorldType lhs, in WorldType rhs)
        {
            return
                lhs.Id == rhs.Id;
        }

        public static bool operator !=(in WorldType lhs, in WorldType rhs)
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
                other is WorldType otherEntity &&
                Equals(otherEntity);
        }

        public bool Equals(WorldType entity)
        {
            return
                Id == entity.Id;
        }
    }
}
