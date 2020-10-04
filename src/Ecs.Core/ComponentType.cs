using System;
using System.Threading;

namespace Ecs.Core
{
    public class ComponentType<T> where T : struct
    {
        static ComponentType()
        {
            ComponentPoolIndex = Interlocked.Increment(ref ComponentPool.PoolCount) - 1;
            Type = typeof(T);
        }

        public static readonly int ComponentPoolIndex;
        public static readonly Type Type;
    }
}
