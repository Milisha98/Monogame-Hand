using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace Hands.Core.Tiles;
internal static class TiledReader
{
    const string DataFile = @"Content\Tilesets\Turret.tmx";

    public static void Load()
    {
        var doc = XElement.Load(DataFile);
        
        string version = doc.Attribute("version").Value;
        if (version != "1.10")
        {
            throw new VersionNotFoundException("This version of TiledReader.cs works with Tiled files of version 1.10.");
        }

        int width  = int.Parse(doc.Attribute("width").Value);
        int height = int.Parse(doc.Attribute("height").Value);
        
        var layers = doc.Elements("layer");

        var worldMap = new WorldMap();
        foreach (XElement layer in layers)
        {
            int layerID = int.Parse(layer.Attribute("id").Value);
            string layerName = layer.Attribute("name").Value;
            var tiles = LoadMapData(layer.Element("data")).ToList();
            
            switch(layerID)
            {
                case 1:
                    worldMap.Floor = tiles; 
                    break;
                //case 2:
                //    worldMap.Walls = tiles; 
                //    break;
                //case 3:
                //    worldMap.WallShadows = tiles;
                //    break;
            }
        }

        Global.World.Map = worldMap;
    }

    private static IEnumerable<Tile> LoadMapData(XElement data)
    {
        var rows = data.Value.Trim('\n').Split('\n');
        for (int y = 0; y < rows.Length; y++)
        {
            var row = rows[y].Trim(',');
            var columns = row.Split(',');
            for (int x = 0; x < columns.Length; x++)
            {
                int frame = int.Parse(columns[x]) - 1;
                if (frame < 0) continue;
                yield return new Tile(new Vector2(x, y), frame);
            }
        }
    }
}



record Tile(Vector2 TilePosition, int Frame)
{
    public Vector2 MapPosition {  get => TilePosition * Global.TileDimension; }
}



