﻿namespace Hunspell.NetCore
{
    public sealed class PrefixEntry : AffixEntry
    {
        public override string Key => Append;
    }
}
