using System;

namespace Ecs.Core
{
    /// <summary>
    /// Fast append0only list.
    /// </summary>
    public class AppendOnlyList<T>
    {
        public T[] Items;
        public int Count;

        public AppendOnlyList(int capacity)
        {
            Items = new T[capacity];
            Count = 0;
        }

        public void Add(T item)
        {
            if (Items.Length == Count)
            {
                Array.Resize(ref Items, Count * 2);
            }

            Items[Count++] = item;
        }
    }
}
