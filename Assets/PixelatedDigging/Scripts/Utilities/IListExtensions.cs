using System.Collections.Generic;

namespace PixelatedDigging.Utilities
{
    public static class IListExtensions
    {
        public static void Populate<T>(this IList<T> list, T value)
        {
            for (int i = 0; i < list.Count; i++)
                list[i] = value;
        }
    }
}