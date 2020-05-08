using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyHelpers.ListExtensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source)
        {
            Random rnd = new Random();
            return source.OrderBy<T, int>((item) => rnd.Next());
        }
    }
}
