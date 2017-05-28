using System.Globalization;

namespace Hunspell.NetCore.Infrastructure
{
    internal static class IntEx
    {
        private static readonly NumberFormatInfo _invariantNumberFormat = CultureInfo.InvariantCulture.NumberFormat;

        public static bool TryParseInvariant(string text, out int value) =>
            int.TryParse(text, NumberStyles.Integer, _invariantNumberFormat, out value);

        public static bool TryParseInvariant(string text, int startIndex, int length, out int value) =>
            int.TryParse(text.Substring(startIndex, length), NumberStyles.Integer, _invariantNumberFormat, out value);

        internal static bool TryParseInvariant(StringSlice text, out int value) =>
            int.TryParse(text.ToString(), NumberStyles.Integer, _invariantNumberFormat, out value);

        public static int? TryParseInvariant(string text)
        {
            int value;
            return TryParseInvariant(text, out value) ? (int?)value : null;
        }
    }
}
