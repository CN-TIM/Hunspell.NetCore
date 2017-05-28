﻿using System.Runtime.CompilerServices;

namespace Hunspell.Infrastructure
{
    internal static class EnumEx
    {
#if !PRE_NETSTANDARD && !DEBUG
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool HasFlag(this AffixConfigOptions value, AffixConfigOptions flag) => (value & flag) == flag;

#if !PRE_NETSTANDARD && !DEBUG
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool HasFlag(this WordEntryOptions value, WordEntryOptions flag) => (value & flag) == flag;

#if !PRE_NETSTANDARD && !DEBUG
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool HasFlag(this AffixEntryOptions value, AffixEntryOptions flag) => (value & flag) == flag;

#if !PRE_NETSTANDARD && !DEBUG
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static bool HasFlag(this AffixReader.EntryListType value, AffixReader.EntryListType flag) => (value & flag) == flag;

#if !PRE_NETSTANDARD && !DEBUG
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool HasFlag(this SpellCheckResultType value, SpellCheckResultType flag) => (value & flag) == flag;

#if !PRE_NETSTANDARD && !DEBUG
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool HasFlag(this NGramOptions value, NGramOptions flag) => (value & flag) == flag;
    }
}
