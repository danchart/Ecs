﻿using System;

namespace Game.Networking
{
    public readonly struct FrameIndex : IEquatable<FrameIndex>
    {
        private readonly ushort _index;

        private FrameIndex(ushort id) => this._index = id;

        public static readonly FrameIndex Nil = new FrameIndex(0);

        public FrameIndex GetNext() => new FrameIndex((this._index == ushort.MaxValue) ? (ushort)1 : (ushort)(this._index + 1));

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
