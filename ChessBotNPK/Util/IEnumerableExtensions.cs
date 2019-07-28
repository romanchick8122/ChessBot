using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace System.Linq
{
    public static class IEnumerableExtensions
    {
        private static Random rng = new Random();
        [Pure]
        public static List<T> RandomShuffle<T>(this IEnumerable<T> enumerable)
        {
            var list = enumerable.ToList();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
}
