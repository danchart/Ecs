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
        /// Resets count to 0.
        /// </summary>
        public static void Clear<T>(this AppendOnlyList<T> list)
        {
            list.Count = 0;
        }

        /// <summary>
        /// Resize the list. Internal call only!!
        /// </summary>
        public static void Resize<T>(this AppendOnlyList<T> list, int count)
        {
            if (count > list.Items.Length)
            {
                Array.Resize(ref list.Items, count);
            }

            // TODO: Shrink list if count < (Items.Length / 2)?

            list.Count = count;
        }

        public static void ShallowCopyTo<T>(this AppendOnlyList<T> source, AppendOnlyList<T> dest)
            where T : struct
        {
            if (dest.Items.Length < source.Count)
            {
                Array.Resize(ref dest.Items, source.Count);
            }

            Array.Copy(source.Items, dest.Items, source.Count);
            dest.Count = source.Count;
        }
    }
}
