using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ClusterClient.Utils
{
    public class ConcurrentDefaultDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TValue> defaultValueFactory;
        private readonly ConcurrentDictionary<TKey, TValue> data;

        public ConcurrentDefaultDictionary(Func<TValue> defaultValueFactory)
        {
            this.defaultValueFactory = defaultValueFactory;
            data = new ConcurrentDictionary<TKey, TValue>();
        }

        public void Add(TKey key, TValue value) => data.AddOrUpdate(key, value, (k, v) => value);

        public TValue Get(TKey key) => data.GetOrAdd(key, defaultValueFactory());

        public TValue this[TKey key]
        {
            get => Get(key);
            set => Add(key, value);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}