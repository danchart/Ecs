using System;

namespace Ecs.Core
{
    public struct Entity : IEquatable<Entity>
    {
        internal World World;
        internal int Id;
        internal uint Version;

        public static bool operator ==(in Entity lhs, in Entity rhs)
        {
            return 
                lhs.Id == rhs.Id && 
                lhs.Version == rhs.Version;
        }

        public static bool operator !=(in Entity lhs, in Entity rhs)
        {
            return 
                lhs.Id != rhs.Id || 
                lhs.Version != rhs.Version;
        }

        public override int GetHashCode()
        {
            return
                HashingUtil.CombineHashCodes(
                    Id,
                    HashingUtil.CombineHashCodes(
                        (int) Version,
                        World.GetHashCode()));
        }

        public override bool Equals(object other)
        {
            return 
                other is Entity otherEntity && 
                Equals(otherEntity);
        }

        public bool Equals(Entity entity)
        {
            return 
                Id == entity.Id && 
                Version == entity.Version && 
                World == entity.World;
        }
    }
}