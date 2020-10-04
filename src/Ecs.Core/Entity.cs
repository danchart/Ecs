using Ecs.Core.src.Hashing;
using System;

namespace Ecs.Core
{
    public struct Entity : IEquatable<Entity>
    {
        public World World;
        public int Id;

        public static bool operator ==(in Entity lhs, in Entity rhs)
        {
            return lhs.Id == rhs.Id;
        }

        public static bool operator !=(in Entity lhs, in Entity rhs)
        {
            return lhs.Id != rhs.Id;
        }

        public override int GetHashCode()
        {
            return
                HashingUtil.CombineHashCodes(
                    Id,
                    World.GetHashCode());
        }

        public override bool Equals(object other)
        {
            return other is Entity otherEntity && Equals(otherEntity);
        }

        public bool Equals(Entity other)
        {
            return Id == other.Id && World == other.World;
        }
    }
}