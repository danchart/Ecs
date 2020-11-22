using System;

namespace Common.Core
{
    public static class RandomHelper
    {
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        public static uint NextUInt()
        {
            uint thirtyBits = (uint)Random.Next(1 << 30);
            uint twoBits = (uint)Random.Next(1 << 2);

            return (thirtyBits << 2) | twoBits;
        }
    }
}
