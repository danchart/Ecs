using System;
using System.Diagnostics;

namespace Common.Core
{
    public struct CircularBufferIndex : IEquatable<CircularBufferIndex>
    {
        private readonly int Index;
        private readonly int Size;

        public CircularBufferIndex(int index, int size)
        {
            this.Index = index % size;
            this.Size = size;
        }

        public static implicit operator int(CircularBufferIndex index) => index.Index;

        public static CircularBufferIndex operator +(in CircularBufferIndex lhs, int offset)
        {
            return lhs.Add(offset);
        }

        public static CircularBufferIndex operator -(in CircularBufferIndex lhs, int offset)
        {
            return lhs.Add(-offset);
        }

        public static bool operator ==(in CircularBufferIndex lhs, in CircularBufferIndex rhs)
        {
            return
                lhs.Index == rhs.Index;
        }

        public static bool operator !=(in CircularBufferIndex lhs, in CircularBufferIndex rhs)
        {
            return
                lhs.Index != rhs.Index;
        }

        public override int GetHashCode() => this.Index;

        public override bool Equals(object other)
        {
            return
                other is CircularBufferIndex otherEntity &&
                Equals(otherEntity);
        }

        public bool Equals(CircularBufferIndex entity)
        {
            return
                this.Index == entity.Index;
        }

        private CircularBufferIndex Add(int offset)
        {
            Debug.Assert(Math.Abs(offset) < this.Size);

            var nextIndex = (this.Index + this.Size + offset) % this.Size;

            return new CircularBufferIndex(nextIndex, this.Size);
        }
    }
}
