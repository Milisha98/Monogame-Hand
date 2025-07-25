using Hands.Core.Managers.Collision;
using Hands.GameObjects.Enemies.Turret;
using Hands.GameObjects.Enemies.SideGun;
using Hands.GameObjects.Enemies.Mobile;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace Hands.Core.Tiles;
internal static class TiledReader
{
    const string DataFile = @"Content\map.tmx";

    public static void Load()
    {
        var doc = XElement.Load(DataFile);

        string version = doc.Attribute("version").Value;
        if (version != "1.10")
        {
            throw new VersionNotFoundException("This version of TiledReader.cs works with Tiled files of version 1.10.");
        }

        int width = int.Parse(doc.Attribute("width").Value);
        int height = int.Parse(doc.Attribute("height").Value);

        var layers = doc.Elements("layer");

        var worldMap = new WorldMap();
        foreach (XElement layer in layers)
        {
            int layerID = int.Parse(layer.Attribute("id").Value);
            string layerName = layer.Attribute("name").Value;
            string layerClass = layer.Attribute("class")?.Value ?? string.Empty;
            var tiles = LoadMapData(layer.Element("data")).ToList();

            switch (layerName)
            {
                case "Floor":
                    worldMap.Floor = tiles;
                    break;
                case "Walls":
                    worldMap.Walls = tiles;
                    break;
                case "Shadows":
                    worldMap.Shadows = tiles;
                    break;
                case "Blocked":
                    CollisionType collisionType = (CollisionType)Enum.Parse(typeof(CollisionType), layerClass, true);
                    var rectangles = tiles.Select(t => new Rectangle((int)t.MapPosition.X, (int)t.MapPosition.Y, Global.TileDimension, Global.TileDimension)).ToList();
                    foreach (var rectangle in rectangles)
                    {
                        var c = new StaticCollision(rectangle, [], collisionType);
                        Global.World.CollisionManager.Register(c);
                    }
                    break;
            }
        }

        ReadTurrets(doc);
        ReadSideGuns(doc);
        ReadMobiles(doc);

        Global.World.Map = worldMap;
    }

    private static void ReadMobiles(XElement doc)
    {
        var objectGroups = doc.Elements("objectgroup");
        foreach (var group in objectGroups)
        {
            string groupName = group.Attribute("name").Value;
            if (groupName != "Mobiles") continue;

            foreach (XElement o in group.Elements("object"))
            {
                MobileInfo m = new
                (
                    ID:             o.Attribute("id").Value,
                    X:              int.Parse(o.Attribute("x").Value),
                    Y:              int.Parse(o.Attribute("y").Value),
                    RoF:            o.ReadPropertyAsFloat("RoF", 2f),
                    WakeDistance:   o.ReadPropertyAsFloat("WakeDistance", Global.World.GlobalWakeDistance),
                    MovementSpeed:  o.ReadPropertyAsFloat("MovementSpeed", 0.2f)
                );

                Global.World.MobileManager.Register(m);
            }
        }
    }

    private static void ReadTurrets(XElement doc)
    {
        var objectGroups = doc.Elements("objectgroup");
        foreach (var group in objectGroups)
        {
            string className = group.Attribute("class")?.Value;
            if (className != "Turret") continue;

            foreach (XElement o in group.Elements("object"))
            {
                string type = o.Attribute("type").Value;
                TurretInfo t = new
                (
                    ID:             o.Attribute("id").Value,
                    X:              int.Parse(o.Attribute("x").Value),
                    Y:              int.Parse(o.Attribute("y").Value) - int.Parse(o.Attribute("height").Value),
                    Style:          (TurretStyle)Enum.Parse(typeof(TurretStyle), type, true),
                    RoF:            o.ReadPropertyAsFloat("RoF", 2f),
                    WakeDistance:   o.ReadPropertyAsFloat("WakeDistance", Global.World.GlobalWakeDistance)
                );

                Global.World.TurretManager.Register(t);
            }
        }
        
    }

    private static void ReadSideGuns(XElement doc)
    {
        var objectGroups = doc.Elements("objectgroup");
        foreach (var group in objectGroups)
        {
            string groupName = group.Attribute("name").Value;
            if (groupName != "SideGuns") continue;

            foreach (XElement o in group.Elements("object"))
            {
                long gid = long.Parse(o.Attribute("gid").Value);
                bool isFlipped = (gid & 0x80000000) != 0;
                gid = gid & 0x0FFFFFFF;

                SideGunInfo sg = new
                (
                    ID: o.Attribute("id").Value,
                    X: int.Parse(o.Attribute("x").Value),
                    Y: int.Parse(o.Attribute("y").Value) - int.Parse(o.Attribute("height").Value),
                    Orientation: isFlipped ? SideGunOrientation.Left : SideGunOrientation.Right,
                    WakeDistance: o.ReadPropertyAsFloat("WakeDistance", Global.World.GlobalWakeDistance)
                );

                Global.World.SideGunManager.Register(sg);
            }
        }
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

internal static class XElementExtensions
{
    internal static string ReadProperty(this XElement parent, string propertyName)
    {
        var properties = parent.Element("properties");
        if (properties == null) return null;
        foreach (XElement property in properties.Elements("property"))
        {
            string name = property.Attribute("name").Value;
            if (name == propertyName)
            {
                string value = property.Attribute("value").Value;
                return value;
            }
        }
        return null;
    }

    internal static float ReadPropertyAsFloat(this XElement parent, string propertyName, float defaultValue = 0f)
    {
        string value = ReadProperty(parent, propertyName);
        if (string.IsNullOrEmpty(value)) return 0f;
        if (float.TryParse(value, out float result)) return result;
        return defaultValue;
    }    

}


public record Tile(Vector2 TilePosition, int Frame)
{
    public Vector2 MapPosition { get => TilePosition * Global.TileDimension; }
}



