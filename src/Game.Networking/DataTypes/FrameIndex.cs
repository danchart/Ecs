using System;
using System.Diagnostics;

namespace Game.Networking
{
    public readonly struct FrameIndex : IEquatable<FrameIndex>
    {
        public static readonly FrameIndex Zero = new FrameIndex(0);
        public static readonly FrameIndex MaxValue = new FrameIndex(ushort.MaxValue);

        private readonly ushort _index;

        private const ushort HalfUshortMaxValue = (ushort.MaxValue / 2) - 1;

        public FrameIndex(ushort index) => this._index = index;

        public FrameIndex GetNext() => new FrameIndex((this._index == ushort.MaxValue) ? (ushort)1 : (ushort)(this._index + 1));

        public bool IsInRange(in FrameIndex startIndex, int length)
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

        public static FrameIndex New(ushort index = 1) => new FrameIndex(index);

        public static implicit operator ushort(FrameIndex id) => id._index;

        public static bool operator ==(in FrameIndex lhs, in FrameIndex rhs)
        {
            return
                lhs._index == rhs._index;
        }

        public static bool operator !=(in FrameIndex lhs, in FrameIndex rhs)
        {
            return
                lhs._index != rhs._index;
        }

        public override int GetHashCode() => this._index;

        public override bool Equals(object other)
        {
            return
                other is FrameIndex otherEntity &&
                Equals(otherEntity);
        }

        public bool Equals(FrameIndex entity)
        {
            return
                this._index == entity._index;
        }
    }
}
