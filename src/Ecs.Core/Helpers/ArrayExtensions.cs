using System;

namespace Ecs.Core.Helpers
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Array.Copy() source array to dest array. Resizes dest when needed.
        /// </summary>
        public static void CopyToResize<T>(this T[] source, T[] dest)
            where T : unmanaged
        {
            if (source.Length > dest.Length)
            {
                Array.Resize(ref dest, source.Length);
            }

            Array.Copy(source, dest, source.Length);
        }
    }
}
