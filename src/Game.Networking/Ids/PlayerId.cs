using Common.Core.Numerics;
using System;

namespace Game.Networking
{
    public struct PlayerId : IEquatable<PlayerId>
    {
        internal readonly int Id;
        internal readonly uint Generation;

        public PlayerId(int id, uint generation = 0)
        {
            Id = id;
            Generation = generation;
        }

        public static bool operator ==(in PlayerId lhs, in PlayerId rhs)
        {
            return
                lhs.Id == rhs.Id &&
                lhs.Generation == rhs.Generation;
        }

        public static bool operator !=(in PlayerId lhs, in PlayerId rhs)
        {
            return
                lhs.Id != rhs.Id ||
                lhs.Generation != rhs.Generation;
        }

        public override int GetHashCode()
        {
            return
                HashCodeHelper.CombineHashCodes(
                    Id,
                    (int)Generation);
        }

        public override bool Equals(object other)
        {
            return
                other is PlayerId otherEntity &&
                Equals(otherEntity);
        }

        public bool Equals(PlayerId entity)
        {
            return
                Id == entity.Id &&
                Generation == entity.Generation;
        }
    }
}
