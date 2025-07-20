using Microsoft.Xna.Framework;

namespace Hands.Core.Sprites;
public static class Size40
{
    public static Vector2 Size      { get => new Vector2(40, 40); }
    public static Vector2 Center    { get => new Vector2(20, 20); }
    public static Vector2 Width     { get => new Vector2(40, 0); }
    public static Vector2 Height    { get => new Vector2(0, 40); }
    public static Point   Point     { get => new Point(40, 40); }
    public static Vector2 HalfWidth => Width / 2;
    public static Vector2 HalfHeight => Height / 2;
}