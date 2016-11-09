using System.Collections.Generic;

namespace MoreExtras.Utils
{
    public static class ListExtensions
    {
        public delegate bool Comparer<T>(T a, T b);
        public static void InsertOrdered<T>(this IList<T> list, T elem, Comparer<T> compare)
        {
            if (0 == list.Count) { list.Add(elem); return; }
            for (int begin = 0, end = list.Count; ;)
            {
                var median = begin + (end - begin) / 2;
                var elemGoesBeforeMedian = compare(elem, list[median]);
                if (median == begin)
                {
                    list.Insert(elemGoesBeforeMedian ? median : median + 1, elem);
                    return;
                }
                if (elemGoesBeforeMedian) end = median; else begin = median;
            }
        }
    }
}
