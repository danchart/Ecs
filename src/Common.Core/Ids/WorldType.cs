using System;

namespace Common.Core
{
    public readonly struct WorldType : IEquatable<WorldType>
    {
        internal readonly string  Name;

        public WorldType(string name)
        {
            Name = name;
        }

        public static implicit operator string(WorldType name) => name.Name;

        public static bool operator ==(in WorldType lhs, in WorldType rhs)
        {
            return
                lhs.Name == rhs.Name;
        }

        public static bool operator !=(in WorldType lhs, in WorldType rhs)
        {
            return
                lhs.Name != rhs.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
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
                Name == entity.Name;
        }
    }
}
