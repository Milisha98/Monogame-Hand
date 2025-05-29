using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.Core;
internal class World
{
    public Camera2D Camera { get; set; } = new Camera2D();

    public WorldMap Map { get; set; } = new();
}
