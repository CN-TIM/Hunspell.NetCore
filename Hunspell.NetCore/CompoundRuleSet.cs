﻿using System.Collections.Generic;
using System.Linq;
using Hunspell.NetCore.Infrastructure;

namespace Hunspell.NetCore
{
    public sealed class CompoundRuleSet : ListWrapper<CompoundRule>
    {
        public static readonly CompoundRuleSet Empty = TakeList(new List<CompoundRule>(0));

        private CompoundRuleSet(List<CompoundRule> rules)
            : base(rules)
        {
        }

        internal static CompoundRuleSet TakeList(List<CompoundRule> rules) =>
            rules == null ? Empty : new CompoundRuleSet(rules);

        public static CompoundRuleSet Create(IEnumerable<CompoundRule> rules) =>
            rules == null ? Empty : TakeList(rules.ToList());

        public bool EntryContainsRuleFlags(WordEntry rv)
        {
            foreach (var rule in Items)
            {
                foreach (var flag in rule)
                {
                    if (!flag.Equals('*') && !flag.Equals('?') && rv.ContainsFlag(flag))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool CompoundCheck(Dictionary<int, WordEntry> words, int wnum, bool all)
        {
            var bt = 0;
            var btinfo = new List<MetacharData>
            {
                new MetacharData()
            };

            foreach (var compoundRule in Items)
            {
                var pp = 0; // pattern position
                var wp = 0; // "words" position
                var ok = true;
                var ok2 = true;
                do
                {
                    while (pp < compoundRule.Count && wp <= wnum)
                    {
                        if (
                            pp + 1 < compoundRule.Count
                            &&
                            (
                                compoundRule[pp + 1] == '*'
                                ||
                                compoundRule[pp + 1] == '?'
                            )
                        )
                        {
                            var wend = compoundRule[pp + 1] == '?' ? wp : wnum;
                            ok2 = true;
                            pp += 2;
                            btinfo[bt].Btpp = pp;
                            btinfo[bt].Btwp = wp;

                            while (wp <= wend)
                            {
                                if (!words[wp].HasFlags || !words[wp].ContainsFlag(compoundRule[pp - 2]))
                                {
                                    ok2 = false;
                                    break;
                                }

                                wp++;
                            }

                            if (wp <= wnum)
                            {
                                ok2 = false;
                            }

                            btinfo[bt].Btnum = wp - btinfo[bt].Btwp;

                            if (btinfo[bt].Btnum > 0)
                            {
                                ++bt;
                                btinfo.Add(new MetacharData());
                            }
                            if (ok2)
                            {
                                break;
                            }
                        }
                        else
                        {
                            ok2 = true;
                            if (
                                words[wp] == null
                                ||
                                !words[wp].HasFlags
                                ||
                                !words[wp].ContainsFlag(compoundRule[pp])
                            )
                            {
                                ok = false;
                                break;
                            }

                            pp++;
                            wp++;

                            if (compoundRule.Count == pp && wp <= wnum)
                            {
                                ok = false;
                            }
                        }
                    }

                    if (ok && ok2)
                    {
                        var r = pp;
                        while (
                            compoundRule.Count > r
                            &&
                            r + 1 < compoundRule.Count
                            &&
                            (
                                compoundRule[r + 1] == '*'
                                ||
                                compoundRule[r + 1] == '?'
                            )
                        )
                        {
                            r += 2;
                        }

                        if (compoundRule.Count <= r)
                        {
                            return true;
                        }
                    }

                    // backtrack
                    if (bt != 0)
                    {
                        do
                        {
                            ok = true;
                            btinfo[bt - 1].Btnum--;
                            pp = btinfo[bt - 1].Btpp;
                            wp = btinfo[bt - 1].Btwp + btinfo[bt - 1].Btnum;
                        }
                        while ((btinfo[bt - 1].Btnum < 0) && (--bt != 0));
                    }

                }
                while (bt != 0);

                if (
                    ok
                    &&
                    ok2
                    &&
                    (
                        !all
                        ||
                        compoundRule.Count <= pp
                    )
                )
                {
                    return true;
                }

                // check zero ending
                while (
                    ok
                    &&
                    ok2
                    &&
                    pp + 1 < compoundRule.Count
                    &&
                    (
                        (compoundRule[pp + 1] == '*')
                        ||
                        (compoundRule[pp + 1] == '?')
                    )
                )
                {
                    pp += 2;
                }

                if (
                    ok
                    &&
                    ok2
                    &&
                    compoundRule.Count <= pp
                )
                {
                    return true;
                }
            }

            return false;
        }

        private class MetacharData
        {
            /// <summary>
            /// Metacharacter (*, ?) position for backtracking.
            /// </summary>
            public int Btpp;
            /// <summary>
            /// Word position for metacharacters.
            /// </summary>
            public int Btwp;
            /// <summary>
            /// Number of matched characters in metacharacter.
            /// </summary>
            public int Btnum;
        }
    }
}
