namespace Hunspell.NetCore.Infrastructure
{
    internal static class Swapper
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }
    }
}
