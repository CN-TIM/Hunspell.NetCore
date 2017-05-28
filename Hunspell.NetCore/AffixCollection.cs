using System;
using System.Collections;
using System.Collections.Generic;
using Hunspell.NetCore.Infrastructure;

namespace Hunspell.NetCore
{
    public sealed class AffixCollection<TEntry> :
        IEnumerable<AffixEntryGroup<TEntry>>
        where TEntry : AffixEntry
    {

        public static readonly AffixCollection<TEntry> Empty = new AffixCollection<TEntry>
        (
            new Dictionary<FlagValue, AffixEntryGroup<TEntry>>(0),
            new Dictionary<char, AffixEntryWithDetailCollection<TEntry>>(0),
            AffixEntryWithDetailCollection<TEntry>.Empty,
            AffixEntryWithDetailCollection<TEntry>.Empty,
            FlagSet.Empty
        );

        private readonly Dictionary<FlagValue, AffixEntryGroup<TEntry>> _affixesByFlag;

        private readonly Dictionary<char, AffixEntryWithDetailCollection<TEntry>> _affixesByIndexedByKey;

        private readonly AffixEntryWithDetailCollection<TEntry> _affixesWithDots;

        private AffixCollection
        (
            Dictionary<FlagValue, AffixEntryGroup<TEntry>> affixesByFlag,
            Dictionary<char, AffixEntryWithDetailCollection<TEntry>> affixesByIndexedByKey,
            AffixEntryWithDetailCollection<TEntry> affixesWithDots,
            AffixEntryWithDetailCollection<TEntry> affixesWithEmptyKeys,
            FlagSet contClasses
        )
        {
            this._affixesByFlag = affixesByFlag;
            this._affixesByIndexedByKey = affixesByIndexedByKey;
            this._affixesWithDots = affixesWithDots;
            AffixesWithEmptyKeys = affixesWithEmptyKeys;
            ContClasses = contClasses;
            HasAffixes = affixesByFlag.Count != 0;
            IsEmpty = !HasAffixes;
        }

        public AffixEntryWithDetailCollection<TEntry> AffixesWithEmptyKeys { get; }

        public FlagSet ContClasses { get; }

        public bool HasAffixes { get; }

        public bool IsEmpty { get; }

        public static AffixCollection<TEntry> Create(List<AffixEntryGroup.Builder<TEntry>> builders)
        {
            if (builders == null || builders.Count == 0)
            {
                return Empty;
            }

            var affixesByFlag = new Dictionary<FlagValue, AffixEntryGroup<TEntry>>(builders.Count);
            var affixesByIndexedByKeyBuilders = new Dictionary<char, List<AffixEntryWithDetail<TEntry>>>();
            var affixesWithDots = new List<AffixEntryWithDetail<TEntry>>();
            var affixesWithEmptyKeys = new List<AffixEntryWithDetail<TEntry>>();
            var contClasses = new HashSet<FlagValue>();

            foreach (var builder in builders)
            {
                var group = builder.ToGroup();
                affixesByFlag.Add(group.AFlag, group);

                foreach (var entry in group.Entries)
                {
                    var key = entry.Key;
                    contClasses.UnionWith(entry.ContClass);
                    var entryWithDetail = new AffixEntryWithDetail<TEntry>(group, entry);
                    if (string.IsNullOrEmpty(key))
                    {
                        affixesWithEmptyKeys.Add(entryWithDetail);
                    }
                    else
                    {
                        if (key.Contains('.'))
                        {
                            affixesWithDots.Add(entryWithDetail);
                        }
                        else
                        {
                            var indexedKey = key[0];
                            List<AffixEntryWithDetail<TEntry>> keyedAffixes;
                            if (!affixesByIndexedByKeyBuilders.TryGetValue(indexedKey, out keyedAffixes))
                            {
                                keyedAffixes = new List<AffixEntryWithDetail<TEntry>>();
                                affixesByIndexedByKeyBuilders.Add(indexedKey, keyedAffixes);

                            }

                            keyedAffixes.Add(entryWithDetail);
                        }
                    }
                }
            }

            var affixesByIndexedByKey = new Dictionary<char, AffixEntryWithDetailCollection<TEntry>>(
                affixesByIndexedByKeyBuilders.Count);
            foreach (var keyedBuilder in affixesByIndexedByKeyBuilders)
            {
                affixesByIndexedByKey.Add(keyedBuilder.Key, AffixEntryWithDetailCollection<TEntry>.TakeList(keyedBuilder.Value));
            }

            return new AffixCollection<TEntry>
            (
                affixesByFlag,
                affixesByIndexedByKey,
                AffixEntryWithDetailCollection<TEntry>.TakeList(affixesWithDots),
                AffixEntryWithDetailCollection<TEntry>.TakeList(affixesWithEmptyKeys),
                FlagSet.Create(contClasses)
            );
        }

        public AffixEntryGroup<TEntry> GetByFlag(FlagValue flag)
        {
            AffixEntryGroup<TEntry> result;
            _affixesByFlag.TryGetValue(flag, out result);
            return result;
        }

        public List<AffixEntryWithDetail<TEntry>> GetMatchingAffixes(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return new List<AffixEntryWithDetail<TEntry>>(0);
            }
            if (typeof(TEntry) == typeof(PrefixEntry))
            {
                return GetMatchingPrefixes(word);
            }
            if (typeof(TEntry) == typeof(SuffixEntry))
            {
                return GetMatchingSuffixes(word);
            }

            throw new NotSupportedException();
        }

        private List<AffixEntryWithDetail<TEntry>> GetMatchingPrefixes(string word)
        {
            var results = new List<AffixEntryWithDetail<TEntry>>();

            AffixEntryWithDetailCollection<TEntry> indexedEntries;
            if (_affixesByIndexedByKey.TryGetValue(word[0], out indexedEntries))
            {
                foreach (var entry in indexedEntries)
                {
                    if (StringEx.IsSubset(entry.Key, word))
                    {
                        results.Add(entry);
                    }
                }
            }

            if (_affixesWithDots.HasItems)
            {
                foreach (var entry in _affixesWithDots)
                {
                    if (StringEx.IsSubset(entry.Key, word))
                    {
                        results.Add(entry);
                    }
                }
            }

            return results;
        }

        private List<AffixEntryWithDetail<TEntry>> GetMatchingSuffixes(string word)
        {
            var results = new List<AffixEntryWithDetail<TEntry>>();

            AffixEntryWithDetailCollection<TEntry> indexedEntries;
            if (_affixesByIndexedByKey.TryGetValue(word[word.Length - 1], out indexedEntries))
            {
                foreach (var entry in indexedEntries)
                {
                    if (StringEx.IsReverseSubset(entry.Key, word))
                    {
                        results.Add(entry);
                    }
                }
            }

            if (_affixesWithDots.HasItems)
            {
                foreach (var entry in _affixesWithDots)
                {
                    if (StringEx.IsReverseSubset(entry.Key, word))
                    {
                        results.Add(entry);
                    }
                }
            }

            return results;
        }

        public IEnumerator<AffixEntryGroup<TEntry>> GetEnumerator()
        {
            return _affixesByFlag.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _affixesByFlag.Values.GetEnumerator();
        }
    }
}
