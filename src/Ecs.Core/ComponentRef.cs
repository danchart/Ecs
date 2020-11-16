namespace Ecs.Core
{
    public struct ComponentRef<T> 
        where T : unmanaged
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
            ref var item = ref _pool.GetItem(ItemIndex);

            ref var entityData = ref _entity.World.GetEntityData(_entity);

            item.Version = _entity.World.State.GlobalVersion;

            return ref item.Item;
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