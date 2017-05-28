using System;
using System.Collections;
using System.Collections.Generic;

namespace Hunspell.NetCore.Infrastructure
{
    public class ArrayWrapper<T> :
        IReadOnlyList<T>
    {
        internal readonly T[] items;

        protected ArrayWrapper(T[] items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            this.items = items;
            HasItems = items.Length != 0;
            IsEmpty = !HasItems;
        }

        public T this[int index] => items[index];

        public int Count => items.Length;

        public bool HasItems { get; }

        public bool IsEmpty { get; }

        public Enumerator GetEnumerator() => new Enumerator(items);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)items).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();

        public struct Enumerator
        {
            private readonly T[] values;
            private int index;

            public Enumerator(T[] values)
            {
                this.values = values;
                index = -1;
            }

            public T Current => values[index];

            public bool MoveNext() => ++index < values.Length;
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

            return ArrayComparer.Equals(x.items, y.items);
        }

        public int GetHashCode(TCollection obj)
        {
            return ArrayComparer.GetHashCode(obj.items);
        }
    }
}
