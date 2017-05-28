using System;
using System.Collections.Generic;
using System.Linq;
using Hunspell.NetCore.Infrastructure;

namespace Hunspell.NetCore
{
    public sealed class FlagSet : ArrayWrapper<FlagValue>
    {
        public static readonly FlagSet Empty = new FlagSet(ArrayEx<FlagValue>.Empty);

        public static readonly ArrayWrapperComparer<FlagValue, FlagSet> DefaultComparer = new ArrayWrapperComparer<FlagValue, FlagSet>();

        private FlagSet(FlagValue[] values)
            : base(values)
        {
        }

        internal static FlagSet TakeArray(FlagValue[] values)
        {
            if (values == null || values.Length == 0)
            {
                return Empty;
            }

            Array.Sort(values);
            return new FlagSet(values);
        }

        public static FlagSet Create(List<FlagValue> given) =>
            given == null ? Empty : TakeArray(given.Distinct().ToArray());

        public static FlagSet Create(IEnumerable<FlagValue> given) =>
            given == null ? Empty : TakeArray(given.Distinct().ToArray());

        public static FlagSet Union(FlagSet a, FlagSet b) => Create(Enumerable.Concat(a, b));

        internal static FlagSet Union(FlagSet set, FlagValue value)
        {
            var valueIndex = Array.BinarySearch(set.Items, value);
            if (valueIndex >= 0)
            {
                return set;
            }

            valueIndex = ~valueIndex; // locate the best insertion point

            var newItems = new FlagValue[set.Items.Length + 1];
            if (valueIndex >= set.Items.Length)
            {
                Array.Copy(set.Items, newItems, set.Items.Length);
                newItems[set.Items.Length] = value;
            }
            else
            {
                Array.Copy(set.Items, newItems, valueIndex);
                Array.Copy(set.Items, valueIndex, newItems, valueIndex + 1, set.Items.Length - valueIndex);
                newItems[valueIndex] = value;
            }

            return new FlagSet(newItems);
        }

        public static bool ContainsAny(FlagSet a, FlagSet b)
        {
            if (a == null || a.IsEmpty || b == null || b.IsEmpty)
            {
                return false;
            }
            if (a.Count == 1)
            {
                return b.Contains(a[0]);
            }
            if (b.Count == 1)
            {
                return a.Contains(b[0]);
            }

            if (a.Count > b.Count)
            {
                Swapper.Swap(ref a, ref b);
            }

            foreach (var item in a)
            {
                if (b.Contains(item))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(FlagValue value) => value.HasValue && Items.Length > 0 && value >= Items[0] && value <= Items[Items.Length -1] && Array.BinarySearch(Items, value) >= 0;

        public bool ContainsAny(FlagSet values) => ContainsAny(this, values);

        public bool ContainsAny(FlagValue a, FlagValue b) =>
                (a.HasValue && Contains(a))
                ||
                (b.HasValue && Contains(b));

        public bool ContainsAny(FlagValue a, FlagValue b, FlagValue c) =>
                (a.HasValue && Contains(a))
                ||
                (b.HasValue && Contains(b))
                ||
                (c.HasValue && Contains(c));

        public bool ContainsAny(FlagValue a, FlagValue b, FlagValue c, FlagValue d) =>
                (a.HasValue && Contains(a))
                ||
                (b.HasValue && Contains(b))
                ||
                (c.HasValue && Contains(c))
                ||
                (d.HasValue && Contains(d));
    }
}
