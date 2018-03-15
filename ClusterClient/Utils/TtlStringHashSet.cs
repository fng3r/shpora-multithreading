using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace ClusterClient.Utils
{
    public class TtlStringHashSet : IEnumerable<string>
    {
        private readonly MemoryCache data = new MemoryCache("cache");

        public bool Add(string item, DateTime expirationDate) => data.Add(item, new object(), expirationDate);

        public bool Contains(string item) => data.Contains(item);

        public void Clear() => data.Trim(100);

        public void Remove(string item) => data.Remove(item);

        public IEnumerator<string> GetEnumerator() => data.Select(pair => pair.Key).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
