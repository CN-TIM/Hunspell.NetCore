using System;
using System.Collections;
using System.Collections.Generic;

namespace Hunspell.NetCore.Infrastructure
{
    public class ListWrapper<T> :
        IReadOnlyList<T>
    {
        protected readonly List<T> Items;

        protected ListWrapper(List<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            this.Items = items;
            HasItems = items.Count != 0;
            IsEmpty = !HasItems;
        }

        public T this[int index] => Items[index];

        public int Count => Items.Count;

        public bool HasItems { get; }

        public bool IsEmpty { get; }

        public Enumerator GetEnumerator() => new Enumerator(Items);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)Items).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

        public struct Enumerator
        {
            private readonly List<T> _values;
            private int _index;

            public Enumerator(List<T> values)
            {
                this._values = values;
                _index = -1;
            }

            public T Current => _values[_index];

            public bool MoveNext() => ++_index < _values.Count;
        }

        public class Comparer : IEqualityComparer<ListWrapper<T>>
        {
            public static readonly Comparer Default = new Comparer();

            public Comparer()
            {
                ListComparer = ListComparer<T>.Default;
            }

            public Comparer(IEqualityComparer<T> valueComparer)
            {
                ListComparer = new ListComparer<T>(valueComparer);
            }

            private ListComparer<T> ListComparer { get; }

            public bool Equals(ListWrapper<T> x, ListWrapper<T> y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }
                if (x == null || y == null)
                {
                    return false;
                }

                return ListComparer.Equals(x.Items, y.Items);
            }

            public int GetHashCode(ListWrapper<T> obj)
            {
                return ListComparer.GetHashCode(obj.Items);
            }
        }
    }
}
