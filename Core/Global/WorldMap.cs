using Hands.Core.Tiles;
using System.Collections.Generic;

namespace Hands.Core;
internal class WorldMap
{
    public List<Tile> Floor { get; set; } = new();
    public List<Tile> Shadows { get; set; } = new();
    //public List<Tile> Walls { get; set; } = new();
    //public List<Tile> WallShadows { get; set; } = new();

}
