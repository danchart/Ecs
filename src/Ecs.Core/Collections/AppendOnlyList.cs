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

    public static class AppendOnlyListExtensions
    {
        /// <summary>
        /// Resize the list. Internal call only!!
        /// </summary>
        internal static void Resize<T>(this AppendOnlyList<T> list, int count)
        {
            if (count > list.Items.Length)
            {
                Array.Resize(ref list.Items, count);
            }

            list.Count = count;
        }

        internal static void ShallowCopyTo<T>(this AppendOnlyList<T> source, AppendOnlyList<T> dest)
            where T : struct
        {
            if (dest.Items.Length < source.Count)
            {
                Array.Resize(ref dest.Items, source.Count);
            }

            Array.Copy(source.Items, dest.Items, source.Count);
        }
    }
}
