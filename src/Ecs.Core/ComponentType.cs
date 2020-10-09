using System;
using System.Threading;

namespace Ecs.Core
{
    public class ComponentType<T> 
        where T : unmanaged
    {
        static ComponentType()
        {
            Index = Interlocked.Increment(ref ComponentPool.PoolCount) - 1;
            Type = typeof(T);
        }

        public static readonly int Index;
        public static readonly Type Type;
    }
}
