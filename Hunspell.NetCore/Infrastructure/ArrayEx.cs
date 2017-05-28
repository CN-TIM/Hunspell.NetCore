using System.Linq;

namespace Hunspell.NetCore.Infrastructure
{
    internal static class ArrayEx<T>
    {
        public static readonly T[] Empty = Enumerable.Empty<T>().ToArray();
    }
}
