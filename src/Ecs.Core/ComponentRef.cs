namespace Ecs.Core
{
    public struct ComponentRef<T> where T : struct
    {
        internal readonly int ItemIndex;

        private readonly ComponentPool<T> _pool;

        private readonly Entity _entity;

        internal ComponentRef(in Entity entity, ComponentPool<T> pool, int itemIndex)
        {
            _entity = entity;
            _pool = pool;
            ItemIndex = itemIndex;
        }

        public ref readonly T UnrefReadOnly()
        {
            return ref _pool.GetItem(ItemIndex).Item;
        }

        public ref T Unref()
        {
            _entity.SetDirty<T>();

            return ref _pool.GetItem(ItemIndex).Item;
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
}