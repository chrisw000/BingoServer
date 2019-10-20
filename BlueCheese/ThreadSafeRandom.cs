using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BlueCheese
{
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }

        public static IReadOnlyList<int> Pick(int take, int max)
        {
            var numbers = new List<int>(Enumerable.Range(1, max));
            numbers.Shuffle();
            return numbers.GetRange(0, take);
        }
    }

    static class MyExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
            n--;
            int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
            }
        }
    }
}
