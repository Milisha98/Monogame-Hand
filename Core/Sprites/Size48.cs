﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.Core.Sprites;
public class Size48
{
    public static Vector2 Size      { get => new Vector2(48, 48); }
    public static Vector2 Center    { get => new Vector2(24, 24); }
    public static Vector2 Width     { get => new Vector2(48, 0); }
    public static Vector2 Height    { get => new Vector2(0, 48); }
    public static Point   Point     { get => new Point(48, 48); }
    public static Vector2 HalfWidth => Width / 2;
    public static Vector2 HalfHeight => Height / 2;
}
