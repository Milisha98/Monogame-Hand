using Hands.Core;
using Hands.Core.Managers.Collision;
using Hands.Core.Sprites;
using Hands.Core.Tiles;
using Hands.Sprites;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace Hands.GameObjects.Tiles;
internal class Tiler : IDraw
{
    private Texture2D _texture;
    private Dictionary<int, SpriteFrame> _frames;

    public void LoadContent(ContentManager contentManager)
    {
        _texture = contentManager.Load<Texture2D>("Tilesets/TileSet32");
        _frames = SpriteHelper.CreateFramesFromTexture(_texture, Size32.Point);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Draw(spriteBatch, Global.World.Map.Floor);
        Draw(spriteBatch, Global.World.Map.Walls);
        Draw(spriteBatch, Global.World.Map.Shadows);
    }

    private void Draw(SpriteBatch spriteBatch, List<Tile> map)
    {
        foreach (Tile tile in map)
        {
            SpriteFrame spriteFrame = _frames[tile.Frame];
            spriteBatch.Draw(_texture, tile.MapPosition, spriteFrame.SourceRectangle, Color.White);
        }
    }
}


