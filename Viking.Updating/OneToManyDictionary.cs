using System;
using System.Collections;
using System.Collections.Generic;

namespace Viking.Updating
{
    internal class OneToManyDictionary<TKey, TValue, TCollection> : IDictionary<TKey, TCollection>
        where TCollection : ICollection<TValue>
    {
        private Func<TCollection> CollectionCreator { get; }
        private Dictionary<TKey, TCollection> Collections { get; } = new Dictionary<TKey, TCollection>();

        public ICollection<TKey> Keys => Collections.Keys;
        public ICollection<TCollection> Values => Collections.Values;

        public int Count => Collections.Count;
        public bool IsReadOnly => false;

        public TCollection this[TKey key] { get => Collections[key]; set => Collections[key] = value; }

        public OneToManyDictionary(Func<TCollection> collectionCreator)
        {
            CollectionCreator = collectionCreator;
        }

        public void Add(TKey key, TCollection value) => Collections.Add(key, value);
        public bool Remove(TKey key) => Collections.Remove(key);
        public void Clear() => Collections.Clear();

        public void Add(TKey key, TValue value)
        {
            if(!Collections.TryGetValue(key, out var collection))
            {
                collection = CollectionCreator();
                Collections.Add(key, collection);
            }
            collection.Add(value);
        }
        public void Remove(TKey key, TValue value)
        {
            if (!Collections.TryGetValue(key, out var collection))
                return;
            collection.Remove(value);
            if(collection.Count <= 0)
                Collections.Remove(key);

        }

        public bool ContainsKey(TKey key) => Collections.ContainsKey(key);
        public bool TryGetValue(TKey key, out TCollection value) => Collections.TryGetValue(key, out value);

        public void Add(KeyValuePair<TKey, TCollection> item) => Collections.Add(item.Key, item.Value);


        public bool Contains(KeyValuePair<TKey, TCollection> item) => Collections.TryGetValue(item.Key, out var res) && ReferenceEquals(item.Value, res);
        public bool Remove(KeyValuePair<TKey, TCollection> item) => ((IDictionary<TKey, TCollection>)Collections).Remove(item);
        public void CopyTo(KeyValuePair<TKey, TCollection>[] array, int arrayIndex) => ((IDictionary<TKey, TCollection>)Collections).CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<TKey, TCollection>> GetEnumerator() => Collections.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Collections.GetEnumerator();
    }
}
