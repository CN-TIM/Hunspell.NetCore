using System;
using System.Text;

namespace Hunspell.NetCore.Infrastructure
{
    internal static class StringBuilderPool
    {
        private const int _maxCachedBuilderCapacity = HunspellDictionary.MaxWordLen;

        [ThreadStatic]
        private static StringBuilder _primaryCache;

        [ThreadStatic]
        private static StringBuilder _secondaryCache;

        [ThreadStatic]
        private static StringBuilder _tertiaryCache;

        public static StringBuilder Get() => GetClearedBuilder();

        public static StringBuilder Get(string value) =>
            GetClearedBuilderWithCapacity(value.Length).Append(value);

        public static StringBuilder Get(string value, int capacity) =>
            GetClearedBuilderWithCapacity(capacity).Append(value);

        public static StringBuilder Get(int capacity) =>
            GetClearedBuilderWithCapacity(capacity);

        public static StringBuilder Get(string value, int valueStartIndex, int valueLength) =>
            Get(value, valueStartIndex, valueLength, valueLength);

        public static StringBuilder Get(string value, int valueStartIndex, int valueLength, int capacity) =>
            GetClearedBuilderWithCapacity(capacity).Append(value, valueStartIndex, valueLength);

        internal static StringBuilder Get(StringSlice value) =>
            Get(value.Text, value.Offset, value.Length, value.Length);

        public static void Return(StringBuilder builder)
        {
            if (builder != null && builder.Capacity <= _maxCachedBuilderCapacity)
            {
                if (_primaryCache == null)
                {
                    _primaryCache = builder;
                }
                else if (_secondaryCache == null)
                {
                    _secondaryCache = builder;
                }
                else
                {
                    _tertiaryCache = builder;
                }
            }
        }

        public static string GetStringAndReturn(StringBuilder builder)
        {
            var value = builder.ToString();
            Return(builder);
            return value;
        }

        private static StringBuilder GetClearedBuilder()
        {
            var result = Steal(ref _primaryCache);
            if (result == null)
            {
                result = Steal(ref _secondaryCache);
                if (result == null)
                {
                    result = Steal(ref _tertiaryCache);
                    if (result == null)
                    {
                        return new StringBuilder();
                    }
                }
            }

            return result.Clear();
        }

        private static StringBuilder GetClearedBuilderWithCapacity(int capacity)
        {
            if (capacity > _maxCachedBuilderCapacity)
            {
                return new StringBuilder(capacity);
            }

            var result = _primaryCache;
            if (result == null || result.Capacity < capacity)
            {
                result = _secondaryCache;
                if (result == null || result.Capacity < capacity)
                {
                    result = _tertiaryCache;
                    if (result == null || result.Capacity < capacity)
                    {
                        return new StringBuilder(capacity);
                    }
                    else
                    {
                        _tertiaryCache = null;
                    }
                }
                else
                {
                    _secondaryCache = null;
                }
            }
            else
            {
                _primaryCache = null;
            }

            return result.Clear();
        }

        private static StringBuilder Steal(ref StringBuilder source)
        {
            var taken = source;
            source = null;
            return taken;
        }
    }
}
