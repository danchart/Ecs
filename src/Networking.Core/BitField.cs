using System;
using System.Diagnostics;

namespace Networking.Core
{
    public struct BitField
    {
        uint _bits;

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

            _bits &= 0xffffffff ^ (1U << index);
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
    }
}
