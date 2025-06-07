using Hands.Core;
using Hands.Core.Tiles;
using Hands.Sprites;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Hands.GameObjects.Tiles;
internal class Tiler : IDraw
{
    private Texture2D _texture;
    private Dictionary<int, SpriteFrame> _frames;

    public void LoadContent(ContentManager contentManager)
    {
        _texture = contentManager.Load<Texture2D>("Tilesets/TileSet32");
        _frames = SpriteHelper.CreateFramesFromTexture(_texture, new Point(Global.TileDimension));
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Draw(spriteBatch, Global.World.Map.Floor);
    }

    private void Draw(SpriteBatch spriteBatch, List<Tile> map)
    {
        spriteBatch.Begin(transformMatrix: Global.World.Camera.ViewMatrix);

        foreach (Tile tile in map)
        {
            SpriteFrame spriteFrame = _frames[tile.Frame];
            spriteBatch.Draw(_texture, tile.MapPosition, spriteFrame.SourceRectangle, Color.White);
        }

        spriteBatch.End();
    }
}
