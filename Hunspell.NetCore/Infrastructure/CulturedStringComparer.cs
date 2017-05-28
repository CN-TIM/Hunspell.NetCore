using System;
using System.Globalization;

namespace Hunspell.NetCore.Infrastructure
{
    //ToDo: Replace this with the framework's built-in stuff
    internal sealed class CulturedStringComparer : StringComparer
    {
        public CulturedStringComparer(CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            Culture = culture;
            CompareInfo = culture.CompareInfo;
        }

        private CultureInfo Culture { get; }

        private CompareInfo CompareInfo { get; }

        public override int Compare(string x, string y) => CompareInfo.Compare(x, y);

        public override bool Equals(string x, string y) => Compare(x, y) == 0;

        public override int GetHashCode(string obj) => StringComparer.CurrentCulture.GetHashCode(obj);
    }
}
