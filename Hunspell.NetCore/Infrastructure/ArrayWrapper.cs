using System;
using System.Collections;
using System.Collections.Generic;

namespace Hunspell.NetCore.Infrastructure
{
    public class ArrayWrapper<T> :
        IReadOnlyList<T>
    {
        internal readonly T[] Items;

        protected ArrayWrapper(T[] items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            this.Items = items;
            HasItems = items.Length != 0;
            IsEmpty = !HasItems;
        }

        public T this[int index] => Items[index];

        public int Count => Items.Length;

        public bool HasItems { get; }

        public bool IsEmpty { get; }

        public Enumerator GetEnumerator() => new Enumerator(Items);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)Items).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

        public struct Enumerator
        {
            private readonly T[] _values;
            private int _index;

            public Enumerator(T[] values)
            {
                this._values = values;
                _index = -1;
            }

            public T Current => _values[_index];

            public bool MoveNext() => ++_index < _values.Length;
        }
    }

    public class ArrayWrapperComparer<TValue, TCollection> : IEqualityComparer<TCollection>
            where TCollection : ArrayWrapper<TValue>
    {
        public ArrayWrapperComparer()
        {
            ArrayComparer = ArrayComparer<TValue>.Default;
        }

        public ArrayWrapperComparer(IEqualityComparer<TValue> valueComparer)
        {
            ArrayComparer = new ArrayComparer<TValue>(valueComparer);
        }

        private ArrayComparer<TValue> ArrayComparer { get; }

        public bool Equals(TCollection x, TCollection y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }

            return ArrayComparer.Equals(x.Items, y.Items);
        }

        public int GetHashCode(TCollection obj)
        {
            return ArrayComparer.GetHashCode(obj.Items);
        }
    }
}
