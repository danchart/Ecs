using System;
using System.Diagnostics;

namespace Game.Networking
{
    public readonly struct FrameIndex : IEquatable<FrameIndex>
    {
        private readonly ushort _index;

        private FrameIndex(ushort id) => this._index = id;

        public static readonly FrameIndex Zero = new FrameIndex(0);
        public static readonly FrameIndex MaxValue = new FrameIndex(ushort.MaxValue);

        public FrameIndex GetNext() => new FrameIndex((this._index == ushort.MaxValue) ? (ushort)1 : (ushort)(this._index + 1));

        public bool IsInRange(in FrameIndex startIndex, int length)
        {
            Debug.Assert(length < (ushort.MaxValue >> 1), "Count must be less than half ushort.MaxValue.");

            // First, get the unchecked unsigned difference of this index with the start index. 
            //
            // IThe difference continues to work for overflows, e.g.:
            //
            //  10 - 65535 = 11
            var uncheckedDiff = (ushort)unchecked(this._index - startIndex);

            return uncheckedDiff <= length;
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
