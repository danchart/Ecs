using System;

namespace Ecs.Core
{
    public struct Entity : IEquatable<Entity>
    {
        internal World World;
        internal int Id;
        internal uint Generation;

        public static bool operator ==(in Entity lhs, in Entity rhs)
        {
            return 
                lhs.Id == rhs.Id && 
                lhs.Generation == rhs.Generation;
        }

        public static bool operator !=(in Entity lhs, in Entity rhs)
        {
            return 
                lhs.Id != rhs.Id || 
                lhs.Generation != rhs.Generation;
        }

        public override int GetHashCode()
        {
            return
                HashingUtil.CombineHashCodes(
                    Id,
                    HashingUtil.CombineHashCodes(
                        (int) Generation,
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
                Generation == entity.Generation && 
                World == entity.World;
        }
    }
}