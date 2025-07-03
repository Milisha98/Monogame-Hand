using Microsoft.Xna.Framework;

namespace Hands.Core.Sprites;
public class Size96
{
    public static Vector2 Size      => new Vector2(96, 96);
    public static Vector2 Center    => new Vector2(48, 48);
    public static Vector2 Width     => new Vector2(96, 0);
    public static Vector2 Height    => new Vector2(0, 96);
    public static Point   Point     => new Point(96, 96);
    public static Vector2 HalfWidth => Width / 2;
    public static Vector2 HalfHeight => Height / 2;
}

