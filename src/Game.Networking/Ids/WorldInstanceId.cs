using System;

namespace Game.Networking
{
    public readonly struct WorldInstanceId : IEquatable<WorldInstanceId>
    {
        internal readonly int Id;

        public WorldInstanceId(int id)
        {
            Id = id;
        }

        public static WorldInstanceId New() => new WorldInstanceId(new Random().Next(0, int.MaxValue));

        public static implicit operator int(WorldInstanceId id) => id.Id;

        public static bool operator ==(in WorldInstanceId lhs, in WorldInstanceId rhs)
        {
            return
                lhs.Id == rhs.Id;
        }

        public static bool operator !=(in WorldInstanceId lhs, in WorldInstanceId rhs)
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
                other is WorldInstanceId otherEntity &&
                Equals(otherEntity);
        }

        public bool Equals(WorldInstanceId entity)
        {
            return
                Id == entity.Id;
        }

        public override string ToString()
        {
            return Id.ToString("x8");
        }
    }
}
