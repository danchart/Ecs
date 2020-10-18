using System;
using System.Diagnostics;

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
            set => Set(0, value);
            get => IsSet(0);
        }

        public bool Bit1
        {
            set => Set(1, value);
            get => IsSet(1);
        }

        public bool Bit2
        {
            set => Set(2, value);
            get => IsSet(2);
        }

        public bool Bit3
        {
            set => Set(3, value);
            get => IsSet(3);
        }

        public bool Bit4
        {
            set => Set(4, value);
            get => IsSet(4);
        }

        public bool Bit5
        {
            set => Set(5, value);
            get => IsSet(5);
        }

        public bool Bit6
        {
            set => Set(6, value);
            get => IsSet(6);
        }

        public bool Bit7
        {
            set => Set(7, value);
            get => IsSet(7);
        }

        public bool Bit8
        {
            set => Set(8, value);
            get => IsSet(8);
        }

        public bool Bit9
        {
            set => Set(9, value);
            get => IsSet(9);
        }

        public bool Bit10
        {
            set => Set(10, value);
            get => IsSet(10);
        }

        public bool Bit11
        {
            set => Set(11, value);
            get => IsSet(11);
        }

        public bool Bit12
        {
            set => Set(12, value);
            get => IsSet(12);
        }

        public bool Bit13
        {
            set => Set(13, value);
            get => IsSet(13);
        }

        public bool Bit14
        {
            set => Set(14, value);
            get => IsSet(14);
        }

        public bool Bit15
        {
            set => Set(15, value);
            get => IsSet(15);
        }

        public bool Bit16
        {
            set => Set(16, value);
            get => IsSet(16);
        }

        public ushort Count()
        {
            ushort count = 0;

            for (ushort i = 0; i < 32; i++)
            {
                count += (ushort)(IsSet(i) ? 1 : 0);
            }

            return count;
        }

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

        public void Unset(ushort index)
        {
            ThrowIfOutOfRange(index);

            _bits &= ~(1U << index);
        }

        public void Toggle(ushort index)
        {
            ThrowIfOutOfRange(index);

            _bits ^= (1U << index);
        }

        /// <summary>
        /// Set all bits up to nth, unsets all others.
        /// </summary>
        public void SetAll(ushort index)
        {
            ThrowIfOutOfRange(index);

            _bits = (1U << index) - 1U;
        }

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
        public static BitField NewSetAll(int index)
        {
            return new BitField((1U << index) - 1U);
        }
    }
}
