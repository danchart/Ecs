using System;

namespace Ecs.Core
{
    internal static class ComponentPool
    {
        public static int PoolCount = 0;
    }

    internal interface IComponentPool
    {
        int New();
        void Free(int index);
        object GetItem(int index);
        Version GetItemVersion(int index);
    }

    internal sealed class ComponentPool<T> : IComponentPool 
        where T : unmanaged
    {
        public ComponentItem<T>[] Items = new ComponentItem<T>[128];

        private int[] _freeItemIndices = new int[128];
        private int _itemCount = 0;
        private int _freeItemCount = 0;

        public int New()
        {
            int newPoolIndex;

            // Use free pool indices first
            if (_freeItemCount > 0)
            {
                newPoolIndex = _freeItemIndices[--_freeItemCount];
            }
            else
            {
                // Resize pool when out-of-space
                if (_itemCount == Items.Length)
                {
                    Array.Resize(ref Items, _itemCount * 2);
                }

                // Current count is new id, then increment.
                newPoolIndex = _itemCount++;
            }

            return newPoolIndex;
        }

        public void Free(int index)
        {
            // Clear item data
            Items[index] = default;

            // Resize free item pool if out-of-space
            if (_freeItemCount == _freeItemIndices.Length)
            {
                Array.Resize(ref _freeItemIndices, _freeItemCount * 2);
            }

            // Add free index to free item pool
            _freeItemIndices[_freeItemCount++] = index;
        }

        //public ComponentRef<T> Reference(int itemIndex)
        //{
        //    return new ComponentRef<T>(this, itemIndex);
        //}

        public Version GetItemVersion(int index)
        {
            return GetItem(index).Version;
        }

        public ref ComponentItem<T> GetItem(int index)
        {
            return ref Items[index];
        }

        object IComponentPool.GetItem(int index)
        {
            return Items[index];
        }

        internal struct ComponentItem<TItem> 
            where TItem : unmanaged
        {
            public TItem Item;
            public Version Version;
        }
    }
}
