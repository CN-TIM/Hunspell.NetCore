﻿namespace Hunspell
{
    public sealed class WordEntry
    {
        public WordEntry(string word, FlagSet flags, MorphSet morphs, WordEntryOptions options)
        {
            Word = word;
            Flags = flags;
            Morphs = morphs;
            Options = options;
        }

        public string Word { get; }

        public FlagSet Flags { get; }

        public MorphSet Morphs { get; }

        public WordEntryOptions Options { get; }

        public bool HasFlags => Flags.HasItems;

        public bool ContainsFlag(FlagValue flag) => Flags.Contains(flag);

        public bool ContainsAnyFlags(FlagValue a, FlagValue b) => HasFlags && Flags.ContainsAny(a, b);

        public bool ContainsAnyFlags(FlagValue a, FlagValue b, FlagValue c) => HasFlags && Flags.ContainsAny(a, b, c);

        public bool ContainsAnyFlags(FlagValue a, FlagValue b, FlagValue c, FlagValue d) => HasFlags && Flags.ContainsAny(a, b, c, d);
    }
}
