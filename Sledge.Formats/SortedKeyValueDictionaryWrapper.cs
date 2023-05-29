using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sledge.Formats
{
    public class SortedKeyValueDictionaryWrapper : IDictionary<string, string>
    {
        private readonly List<KeyValuePair<string, string>> _list;

        public int Count => Keys.Count;
        public bool IsReadOnly => false;

        public ICollection<string> Keys => _list.Select(x => x.Key.ToLowerInvariant()).ToList();
        public ICollection<string> Values => Keys.Select(x => this[x]).ToList();

        public SortedKeyValueDictionaryWrapper(List<KeyValuePair<string, string>> list)
        {
            _list = list;
        }

        public void Add(KeyValuePair<string, string> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return _list.Any(x => string.Equals(x.Key, item.Key, StringComparison.InvariantCultureIgnoreCase) && Equals(x.Value, item.Value));
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return _list.Remove(item);
        }

        public string this[string key]
        {
            get
            {
                foreach (var kv in _list)
                {
                    if (kv.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)) return kv.Value;
                }
                return "";
            }
            set
            {
                Remove(key);
                Add(key, value);
            }
        }

        public void Add(string key, string value)
        {
            if (ContainsKey(key)) throw new ArgumentException("Key already exists", nameof(key));
            _list.Add(new KeyValuePair<string, string>(key, value));
        }

        public bool ContainsKey(string key)
        {
            return _list.Any(x => string.Equals(x.Key, key, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool Remove(string key)
        {
            return _list.RemoveAll(x => string.Equals(x.Key, key, StringComparison.InvariantCultureIgnoreCase)) > 0;
        }

        public bool TryGetValue(string key, out string value)
        {
            if (ContainsKey(key))
            {
                value = this[key];
                return true;
            }

            value = default;
            return false;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
