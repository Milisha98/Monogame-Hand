using Microsoft.Xna.Framework;

namespace Hands.Core.Sprites;
public class Size200
{
    public static Vector2 Size      { get => new Vector2(200, 200); }
    public static Vector2 Center    { get => new Vector2(100, 100); }
    public static Vector2 Width     { get => new Vector2(200, 0); }
    public static Vector2 Height    { get => new Vector2(0, 200); }
    public static Point   Point     { get => new Point(200, 200); }
    public static Vector2 HalfWidth => Width / 2;
    public static Vector2 HalfHeight => Height / 2;
}

