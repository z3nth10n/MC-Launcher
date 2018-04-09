using System;
using System.Collections.Generic;

namespace LauncherAPI
{
    public enum OS { Windows, Linux, OSx, Other }

    public static class ApiExtensions
    {
        public static void ForEach<T>(
this IEnumerable<T> source,
Action<T> action)
        {
            foreach (T element in source)
                action(element);
        }

        public static void ForEachStop<T>(
this IEnumerable<T> source,
Func<T, bool> action)
        {
            foreach (T element in source)
                if (action(element))
                    break;
        }

        public static bool Between(this int val, int min, int max, bool exclusive = false)
        {
            if (!exclusive)
                return val >= min && val <= max;
            else
                return val > min && val < max;
        }

        public static bool Between(this int val, long min, long max, bool exclusive = false)
        {
            if (!exclusive)
                return val >= min && val <= max;
            else
                return val > min && val < max;
        }

        public static bool Between(this long val, long min, long max, bool exclusive = false)
        {
            if (!exclusive)
                return val >= min && val <= max;
            else
                return val > min && val < max;
        }
    }
}