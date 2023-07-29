using System.Collections.Generic;

namespace Sledge.Formats.Bsp.Builder
{
    /// <summary>
    /// Mirrors a list(t) into a dictionary(t,int) for faster equality and index lookups.
    /// Doubles memory usage of the list but the speed gain is worth it for large collections.
    /// </summary>
    internal class ListDictionaryWrapper<T>
    {
        private readonly IList<T> _list;
        private readonly Dictionary<T, int> _indexes;

        public ListDictionaryWrapper(IList<T> list, IEqualityComparer<T> comparer = null)
        {
            _list = list;
            _indexes = new Dictionary<T, int>(comparer);
            for (var i = 0; i < list.Count; i++)
            {
                _indexes.Add(list[i], i);
            }
        }

        /// <summary>
        /// Gets the index of the provided item, or -1 if it's not present
        /// </summary>
        public int Get(T item)
        {
            return _indexes.TryGetValue(item, out var idx) ? idx : -1;
        }

        /// <summary>
        /// Adds an item to the list if it's not already there and returns the index of the item in the list.
        /// </summary>
        public int AddOrGet(T item)
        {
            if (!_indexes.TryGetValue(item, out var idx))
            {
                idx = _list.Count;
                _list.Add(item);
                _indexes.Add(item, idx);
                return idx;
            }
            return idx;
        }
    }
}
