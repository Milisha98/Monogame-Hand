namespace Hands.Core.Sprites;
internal class Size8
{
    public static Vector2 Size            { get => new Vector2(8, 8); }
    public static Vector2 Center          { get => new Vector2(4, 4); }
    public static Vector2 Width           { get => new Vector2(8, 0); }
    public static Vector2 Height          { get => new Vector2(0, 8); }
    public static Point Point             { get => new Point(8, 8); }
    public static Vector2 HalfWidth       => Width / 2;
    public static Vector2 HalfHeight      => Height / 2;
}
