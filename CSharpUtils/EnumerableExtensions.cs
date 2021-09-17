using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUtils
{
    public static class EnumerableExtensions
    {
        public static (IEnumerable<T> whenTrue, IEnumerable<T> whenFalse) Partition<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var splitByPasses = source.ToLookup(predicate);
            return (splitByPasses[true], splitByPasses[false]);
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source) => 
            source == null || !source.Any();

        public static IEnumerable<TState> Scan<T, TState>(this IEnumerable<T> source, TState state, Func<TState, T, TState> folder)
        {
            var currentState = state;
            var accumulatingStates = new List<TState> { state };

            foreach (var element in source)
            {
                var nextState = folder(currentState, element);
                accumulatingStates.Add(nextState);
                currentState = nextState;
            }

            return accumulatingStates;
        }
    }
}
