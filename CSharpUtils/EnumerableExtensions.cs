using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUtils
{
    public static class EnumerableExtensions
    {
        public static (IEnumerable<T> whenTrue, IEnumerable<T> whenFalse) Partition<T>(this IEnumerable<T> xs, Func<T, bool> predicate)
        {
            var splitByPasses = xs.ToLookup(predicate);
            return (splitByPasses[true], splitByPasses[false]);
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> xs) => xs == null || !xs.Any();
    }
}
