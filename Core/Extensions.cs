using System.Linq;

namespace Hands.Core;
public static class Extensions
{
    public static bool In<T>(this T state, params T[] states) => states.Contains(state);
}
