using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.Core.Sprites;
internal class Size64
{
    public static Vector2 Center    { get => new Vector2(32, 32); }
    public static Vector2 Width     { get => new Vector2(64, 0); }
    public static Vector2 Height    { get => new Vector2(0, 64); }
    public static Point Point       { get => new Point(64, 64); }
    public static Vector2 HalfWidth       => Width / 2;
    public static Vector2 HalfHeight      => Height / 2;
}
