using System;
using System.Threading;

namespace Ecs.Core
{
    public struct ComponentRef<T> where T : struct
    {
        public readonly int ItemIndex;

        private readonly ComponentPool<T> _pool;

        public ComponentRef(ComponentPool<T> pool, int poolIndex)
        {
            _pool = pool;
            ItemIndex = poolIndex;
        }

        public ref T Unref()
        {
            return ref _pool.GetItem(ItemIndex);
        }

        public static bool operator ==(in ComponentRef<T> lhs, in ComponentRef<T> rhs)
        {
            return lhs.ItemIndex == rhs.ItemIndex && lhs._pool == rhs._pool;
        }

        public static bool operator !=(in ComponentRef<T> lhs, in ComponentRef<T> rhs)
        {
            return lhs.ItemIndex != rhs.ItemIndex || lhs._pool != rhs._pool;
        }

        public override bool Equals(object obj)
        {
            return obj is ComponentRef<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ItemIndex;
        }
    }

    //public sealed class DataRef<T>
    //{
    //    private readonly Func<T> getter;
    //    private readonly Action<T> setter;

    //    public DataRef(Func<T> getter, Action<T> setter)
    //    {
    //        this.getter = getter;
    //        this.setter = setter;
    //    }

    //    public T Value 
    //    { 
    //        get => getter();
    //        set => setter(value);
    //    }

    //    public ref T Unref()
    //    {
    //        return ref Pool.Items[Idx];
    //    }
    //}
}