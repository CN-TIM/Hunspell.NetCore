﻿using System.Collections.Generic;
using System.Linq;
using Hunspell.NetCore.Infrastructure;

namespace Hunspell.NetCore
{
    public sealed class SingleReplacementSet : ListWrapper<SingleReplacement>
    {
        public static readonly SingleReplacementSet Empty = TakeList(new List<SingleReplacement>(0));

        private SingleReplacementSet(List<SingleReplacement> replacements)
            : base(replacements)
        {
        }

        internal static SingleReplacementSet TakeList(List<SingleReplacement> replacements) =>
            replacements == null ? Empty : new SingleReplacementSet(replacements);

        public static SingleReplacementSet Create(IEnumerable<SingleReplacement> replacements) =>
            replacements == null ? Empty : TakeList(replacements.ToList());
    }
}
