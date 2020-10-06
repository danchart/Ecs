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

    internal class ComponentPool<T> : IComponentPool where T : struct
    {
        private ComponentItem<T>[] _items = new ComponentItem<T>[128];
        int[] _freeItemIndices = new int[128];
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
                if (_itemCount == _items.Length)
                {
                    Array.Resize(ref _items, _itemCount * 2);
                }

                // Current count is new id, then increment.
                newPoolIndex = _itemCount++;
            }

            return newPoolIndex;
        }

        public void Free(int index)
        {
            // Clear item data
            _items[index] = default;

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
            return ref _items[index];
        }

        object IComponentPool.GetItem(int index)
        {
            return _items[index];
        }

        internal struct ComponentItem<TItem> where TItem : struct
        {
            public TItem Item;
            public Version Version;
        }
    }
}
