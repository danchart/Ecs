using System;

namespace Game.Networking
{
    public struct PlayerId : IEquatable<PlayerId>
    {
        internal readonly int Id;

        public PlayerId(int id)
        {
            Id = id;
        }

        public static implicit operator int(PlayerId id) => id.Id;

        public static bool operator ==(in PlayerId lhs, in PlayerId rhs)
        {
            return
                lhs.Id == rhs.Id;
        }

        public static bool operator !=(in PlayerId lhs, in PlayerId rhs)
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
                other is PlayerId otherEntity &&
                Equals(otherEntity);
        }

        public bool Equals(PlayerId entity)
        {
            return
                Id == entity.Id;
        }
    }
}
