using System.Linq;

namespace Hands.Core;
public static class Extensions
{
    public static bool In<T>(this T state, params T[] states) => states.Contains(state);

    public static Rectangle Move(this Rectangle rectangle, Point point) => new Rectangle(point, rectangle.Size);
    public static Rectangle Move(this Rectangle rectangle, Vector2 vector) => new Rectangle(vector.ToPoint(), rectangle.Size);

}
