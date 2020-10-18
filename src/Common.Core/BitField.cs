using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Common.Core
{
    public struct BitField
    {
        private uint _bits;

        public BitField(uint value = 0)
        {
            _bits = value;
        }

        public bool Bit0
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(0, value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsSet(0);
        }

        public bool Bit1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(1, value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsSet(1);
        }

        public bool Bit2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(2, value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsSet(2);
        }

        public bool Bit3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(3, value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsSet(3);
        }

        public bool Bit4
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(4, value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsSet(4);
        }

        public bool Bit5
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(5, value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsSet(5);
        }

        public bool Bit6
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(6, value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsSet(6);
        }

        public bool Bit7
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(7, value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsSet(7);
        }

        public bool Bit8
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(8, value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsSet(8);
        }

        public bool Bit9
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(9, value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsSet(9);
        }

        public bool Bit10
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(10, value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsSet(10);
        }

        public bool Bit11
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(11, value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsSet(11);
        }

        public bool Bit12
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(12, value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsSet(12);
        }

        public bool Bit13
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(13, value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsSet(13);
        }

        public bool Bit14
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(14, value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsSet(14);
        }

        public bool Bit15
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(15, value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsSet(15);
        }

        public bool Bit16
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(16, value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsSet(16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort Count()
        {
            ushort count = 0;

            for (ushort i = 0; i < 32; i++)
            {
                count += (ushort)(IsSet(i) ? 1 : 0);
            }

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(ushort index, bool value = true)
        {
            ThrowIfOutOfRange(index);

            if (value)
            {
                _bits |= (1U << index);
            }
            else
            {
                Unset(0);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unset(ushort index)
        {
            ThrowIfOutOfRange(index);

            _bits &= ~(1U << index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Toggle(ushort index)
        {
            ThrowIfOutOfRange(index);

            _bits ^= (1U << index);
        }

        /// <summary>
        /// Set all bits up to nth, unsets all others.
        /// </summary>
        /// [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void SetAll(ushort index)
        {
            ThrowIfOutOfRange(index);

            _bits = (1U << index) - 1U;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSet(ushort index)
        {
            ThrowIfOutOfRange(index);

            return (_bits & (1 << index)) != 0;
        }

        [Conditional("DEBUG")]
        private static void ThrowIfOutOfRange(ushort index)
        {
            if (index >= 32)
            {
                throw new IndexOutOfRangeException($"{index} out of range.");
            }
        }

        /// <summary>
        /// Creats bitfield with all bits set up to and including n.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitField NewSetAll(int index)
        {
            return new BitField((1U << index) - 1U);
        }
    }
}
