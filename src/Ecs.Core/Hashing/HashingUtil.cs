﻿namespace Ecs.Core.src.Hashing
{
    public static class HashingUtil
    {
        public static int CombineHashCodes(int h1, int h2)
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 23 + h1;
                hash = hash * 23 + h2;

                return hash;
            }
        }
    }
}
