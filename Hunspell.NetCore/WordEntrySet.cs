using System;
using System.Collections.Generic;
using System.Linq;
using Hunspell.NetCore.Infrastructure;

namespace Hunspell.NetCore
{
    public sealed class WordEntrySet : ArrayWrapper<WordEntry>
    {
        public static readonly WordEntrySet Empty = TakeArray(ArrayEx<WordEntry>.Empty);

        private WordEntrySet(WordEntry[] entries)
            : base(entries)
        {
        }

        internal static WordEntrySet TakeArray(WordEntry[] entries) => entries == null ? Empty : new WordEntrySet(entries);

        public static WordEntrySet Create(IEnumerable<WordEntry> entries) => entries == null ? Empty : TakeArray(entries.ToArray());

        public static WordEntrySet CopyWithItemReplaced(WordEntrySet source, int index, WordEntry replacement)
        {
            var newEntries = new WordEntry[source.Items.Length];
            Array.Copy(source.Items, newEntries, newEntries.Length);
            newEntries[index] = replacement;
            return TakeArray(newEntries);
        }

        public static WordEntrySet CopyWithItemAdded(WordEntrySet source, WordEntry entry)
        {
            WordEntry[] newEntries;
            if (source.Items.Length == 0)
            {
                newEntries = new[] { entry };
            }
            else
            {
                newEntries = new WordEntry[source.Items.Length + 1];
                Array.Copy(source.Items, newEntries, source.Items.Length);
                newEntries[source.Items.Length] = entry;
            }

            return TakeArray(newEntries);
        }

        public WordEntry FirstOrDefault()
        {
            return Items.Length != 0 ? Items[0] : null;
        }

        internal void DestructiveReplace(int index, WordEntry entry)
        {
            Items[index] = entry;
        }
    }
}
