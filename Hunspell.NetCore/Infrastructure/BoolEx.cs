namespace Hunspell.NetCore.Infrastructure
{
    internal static class BoolEx
    {
        public static bool PostfixIncrement(ref bool b)
        {
            if (b)
            {
                return true;
            }
            b = true;
            return false;
        }
    }
}
