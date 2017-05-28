using System;
using System.Collections.Generic;

namespace Hunspell.NetCore.Infrastructure
{
    internal sealed class StringDeduper
    {
        public StringDeduper()
            : this(StringComparer.Ordinal)
        {
        }

        public StringDeduper(IEqualityComparer<string> comparer)
        {
            _lookup = new Dictionary<string, string>(comparer);
            Add(string.Empty);
        }

        private readonly Dictionary<string, string> _lookup;

        public string GetEqualOrAdd(string item)
        {
            string existing;
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

        public void Add(string item)
        {
            string existing;
            if (!_lookup.TryGetValue(item, out existing))
            {
                _lookup[item] = item;
            }
        }
    }
}
