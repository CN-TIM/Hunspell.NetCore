using System.Collections.Generic;
using System.Linq;
using Hunspell.NetCore.Infrastructure;

namespace Hunspell.NetCore
{
    public sealed class WarningList : ListWrapper<string>
    {
        private WarningList(List<string> warnings)
            : base(warnings)
        {
        }

        internal static WarningList TakeList(List<string> warnings) =>
            new WarningList(warnings ?? new List<string>(0));

        public static WarningList Create(IEnumerable<string> warnings) => warnings == null ? TakeList(null) : TakeList(warnings.ToList());
    }
}
