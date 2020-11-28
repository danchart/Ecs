using System;
using System.Diagnostics;

namespace Game.Networking
{
    public readonly struct FrameNumber : IEquatable<FrameNumber>
    {
        public static readonly FrameNumber Zero = new FrameNumber(0);
        public static readonly FrameNumber MaxValue = new FrameNumber(ushort.MaxValue);

        private readonly ushort _index;

        private const ushort HalfUshortMaxValue = (ushort.MaxValue / 2) - 1;

        public FrameNumber(ushort index) => this._index = index;

        public static FrameNumber operator +(in FrameNumber lhs, ushort offset) => 
            new FrameNumber((ushort)unchecked(lhs._index + offset));

        public static FrameNumber operator -(in FrameNumber lhs, ushort offset) =>
            new FrameNumber((ushort)unchecked(lhs._index - offset));

        public bool IsInRange(in FrameNumber startIndex, int length)
        {
            Debug.Assert(length < (ushort.MaxValue >> 1), "Count must be less than half ushort.MaxValue.");

            // First, get the unchecked unsigned difference of this index with the start index. 
            //
            // The difference continues to work for overflow, e.g.:
            //
            //  10 - 65535 = 11
            var uncheckedDiff = (ushort)unchecked(this._index - startIndex);

            return uncheckedDiff <= length;
        }

        /// <summary>
        /// Compares two FrameIndex values even with rollover. Fails if difference is greater than window which itself
        /// can be no more than 0.5 * ushort.MaxValue - 1.
        /// </summary>
        public static int Compare(in ushort left, in ushort right, int window = HalfUshortMaxValue - 1)
        {
            var uncheckedDiff = (ushort)unchecked(right - left);

            if (uncheckedDiff == 0)
            {
                // left == right
                return 0;
            }

            if (uncheckedDiff <= window)
            {
                // left < right
                return 1;
            }

            // left > right
            return -1;
        }

        public static FrameNumber New(ushort index = 1) => new FrameNumber(index);

        public static implicit operator ushort(FrameNumber id) => id._index;

        public static bool operator ==(in FrameNumber lhs, in FrameNumber rhs)
        {
            return
                lhs._index == rhs._index;
        }

        public static bool operator !=(in FrameNumber lhs, in FrameNumber rhs)
        {
            return
                lhs._index != rhs._index;
        }

        public override int GetHashCode() => this._index;

        public override bool Equals(object other)
        {
            return
                other is FrameNumber otherEntity &&
                Equals(otherEntity);
        }

        public bool Equals(FrameNumber entity)
        {
            return
                this._index == entity._index;
        }
    }
}
