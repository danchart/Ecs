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
        void CopyTo(IComponentPool targetPool);
        IComponentPool Clone();
    }

    internal sealed class ComponentPool<T> : IComponentPool 
        where T : unmanaged
    {
        public ComponentItem<T>[] Items;
        private int[] _freeItemIndices;

        private int _itemCount = 0;
        private int _freeItemCount = 0;

        internal ComponentPool(int capacity)
        {
            Items = new ComponentItem<T>[capacity];
            _freeItemIndices = new int[capacity];
        }

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

        public void CopyTo(IComponentPool targetPool)
        {
            CopyTo((ComponentPool<T>)targetPool);
        }

        public IComponentPool Clone()
        {
            var pool = new ComponentPool<T>(Items.Length);

            CopyTo(pool);

            return pool;
        }

        /// <summary>
        /// Copies this component pool into the target pool.
        /// </summary>
        public void CopyTo(ComponentPool<T> targetPool)
        {
            if (targetPool.Items.Length < this._itemCount)
            {
                Array.Resize(ref targetPool.Items, this._itemCount);
            }

            Array.Copy(this.Items, targetPool.Items, this._itemCount);

            if (targetPool._freeItemIndices.Length < this._freeItemCount)
            {
                Array.Resize(ref targetPool._freeItemIndices, this._freeItemCount);
            }

            Array.Copy(_freeItemIndices, targetPool._freeItemIndices, _freeItemCount);

            targetPool._itemCount = this._itemCount;
            targetPool._freeItemCount = this._freeItemCount;
        }

        internal struct ComponentItem<TItem> 
            where TItem : unmanaged
        {
            public TItem Item;
            public Version Version;
        }
    }
}
