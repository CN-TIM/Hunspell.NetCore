using System.Collections.Generic;

namespace Hunspell.NetCore.Infrastructure
{
    internal sealed class Deduper<T>
    {
        public Deduper(IEqualityComparer<T> comparer)
        {
            _lookup = new Dictionary<T, T>(comparer);
        }

        private readonly Dictionary<T, T> _lookup;

        public T GetEqualOrAdd(T item)
        {
            T existing;
            if (_lookup.TryGetValue(item, out existing))
            {
                return existing;
            }
            else
            {
                _lookup[item] = item;
                return item;
            }
        }

        public void Add(T item)
        {
            T existing;
            if (!_lookup.TryGetValue(item, out existing))
            {
                _lookup[item] = item;
            }
        }
    }
}
