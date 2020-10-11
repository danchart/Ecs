using System;

namespace Ecs.Core
{
    public static class FloatExtensions
    {
        // ******************************************************************
        // Base on Hans Passant Answer on:
        // https://stackoverflow.com/questions/2411392/double-epsilon-for-equality-greater-than-less-than-less-than-or-equal-to-gre

        //public static bool AboutEquals(this float value1, float value2)
        //{
        //    float epsilon = Math.Max(Math.Abs(value1), Math.Abs(value2)) * 1E-6f;
        //    return Math.Abs(value1 - value2) <= epsilon;
        //}

        // ******************************************************************
        // Base on Hans Passant Answer on:
        // https://stackoverflow.com/questions/2411392/double-epsilon-for-equality-greater-than-less-than-less-than-or-equal-to-gre

        public static bool AboutEquals(
            this float value1, 
            float value2, 
            float precalculatedContextualEpsilon = 1E-6f)
        {
            return Math.Abs(value1 - value2) <= precalculatedContextualEpsilon;
        }
    }
}
