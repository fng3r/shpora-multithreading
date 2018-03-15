using System.Collections.Generic;

namespace ClusterClient.Extensions
{
    public static class LinqExtensions
    {
        public static Queue<T> ToQueue<T>(this IEnumerable<T> source) => new Queue<T>(source);
    }
}
