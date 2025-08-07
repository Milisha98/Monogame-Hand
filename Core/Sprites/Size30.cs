using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.Core.Sprites;
public static class Size30
{
    public static Vector2 Size      { get => new Vector2(30, 30); }
    public static Vector2 Center    { get => new Vector2(15, 15); }
    public static Vector2 Width     { get => new Vector2(30, 0); }
    public static Vector2 Height    { get => new Vector2(0, 30); }
    public static Point Point       { get => new Point(30, 30); }
    public static Vector2 HalfWidth       => Width / 2;
    public static Vector2 HalfHeight      => Height / 2;
}
