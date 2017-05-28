using System;
using System.Collections.Generic;
using Hunspell.NetCore.Infrastructure;

namespace Hunspell.NetCore
{
    public sealed class MultiReplacementEntry : ReplacementEntry
    {
        public MultiReplacementEntry(string pattern)
            : base(pattern)
        {
        }

        public MultiReplacementEntry(string pattern, ReplacementValueType type, string value)
            : base(pattern)
        {
            if (type == ReplacementValueType.Med)
            {
                _med = value;
            }
            else if (type == ReplacementValueType.Ini)
            {
                _ini = value;
            }
            else if (type == ReplacementValueType.Fin)
            {
                _fin = value;
            }
            else if (type == ReplacementValueType.Isol)
            {
                _isol = value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        private string _med;
        private string _ini;
        private string _fin;
        private string _isol;

        public override string Med => _med;

        public override string Ini => _ini;

        public override string Fin => _fin;

        public override string Isol => _isol;

        public override string this[ReplacementValueType type]
        {
            get
            {
                if (type == ReplacementValueType.Med)
                {
                    return _med;
                }
                if (type == ReplacementValueType.Ini)
                {
                    return _ini;
                }
                if (type == ReplacementValueType.Fin)
                {
                    return _fin;
                }
                if (type == ReplacementValueType.Isol)
                {
                    return _isol;
                }

                throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        public MultiReplacementEntry With(ReplacementValueType type, string value)
        {
            var result = new MultiReplacementEntry(Pattern);

            if (type == ReplacementValueType.Med)
            {
                result._med = value;
                result._ini = _ini;
                result._fin = _fin;
                result._isol = _isol;
            }
            else if (type == ReplacementValueType.Ini)
            {
                result._med = _med;
                result._ini = value;
                result._fin = _fin;
                result._isol = _isol;
            }
            else if (type == ReplacementValueType.Fin)
            {
                result._med = _med;
                result._ini = _ini;
                result._fin = value;
                result._isol = _isol;
            }
            else if (type == ReplacementValueType.Isol)
            {
                result._med = _med;
                result._ini = _ini;
                result._fin = _fin;
                result._isol = value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            return result;
        }
    }

    internal static class MultiReplacementEntryExtensions
    {
        public static bool AddReplacementEntry(this Dictionary<string, MultiReplacementEntry> list, string pattern1, string pattern2)
        {
            if (string.IsNullOrEmpty(pattern1) || pattern2 == null)
            {
                return false;
            }

            var pattern1Builder = StringBuilderPool.Get(pattern1);
            ReplacementValueType type;
            var trailingUnderscore = pattern1Builder.EndsWith('_');
            if (pattern1Builder.StartsWith('_'))
            {
                if (trailingUnderscore)
                {
                    type = ReplacementValueType.Isol;
                    pattern1Builder.Remove(pattern1Builder.Length - 1, 1);
                }
                else
                {
                    type = ReplacementValueType.Ini;
                }

                pattern1Builder.Remove(0, 1);
            }
            else
            {
                if (trailingUnderscore)
                {
                    type = ReplacementValueType.Fin;
                    pattern1Builder.Remove(pattern1Builder.Length - 1, 1);
                }
                else
                {
                    type = ReplacementValueType.Med;
                }
            }

            pattern1Builder.Replace('_', ' ');

            pattern1 = StringBuilderPool.GetStringAndReturn(pattern1Builder);
            pattern2 = pattern2.Replace('_', ' ');

            // find existing entry
            MultiReplacementEntry entry;
            if (list.TryGetValue(pattern1, out entry))
            {
                entry = entry.With(type, pattern2);
            }
            else
            {
                // make a new entry if none exists
                entry = new MultiReplacementEntry(pattern1, type, pattern2);
            }

            list[pattern1] = entry;

            return true;
        }
    }
}
