namespace Hunspell.NetCore.Infrastructure
{
    internal static class EnumEx
    {
        public static bool HasFlag(this AffixConfigOptions value, AffixConfigOptions flag) => (value & flag) == flag;

        public static bool HasFlag(this WordEntryOptions value, WordEntryOptions flag) => (value & flag) == flag;

        public static bool HasFlag(this AffixEntryOptions value, AffixEntryOptions flag) => (value & flag) == flag;

        internal static bool HasFlag(this AffixReader.EntryListType value, AffixReader.EntryListType flag) => (value & flag) == flag;

        public static bool HasFlag(this SpellCheckResultType value, SpellCheckResultType flag) => (value & flag) == flag;

        public static bool HasFlag(this NGramOptions value, NGramOptions flag) => (value & flag) == flag;
    }
}
