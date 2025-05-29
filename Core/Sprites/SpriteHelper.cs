using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Hands.Sprites;

internal static class SpriteHelper
{
    public static Dictionary<int, SpriteFrame> CreateFramesFromTexture(this Texture2D texture, Point spriteDimensions)
    {
        var textures = new Dictionary<int, SpriteFrame>();
        int count = 0;
        for (int y = 0; y < texture.Height; y += spriteDimensions.Y)
        {
            for (int x = 0; x < texture.Width; x += spriteDimensions.X)
            {
                Rectangle sourceRectangle = new(x, y, spriteDimensions.X, spriteDimensions.Y);
                var frame = new SpriteFrame(count, sourceRectangle);
                textures.Add(count, frame);
                count++;
            }
        }
        return textures;
    }

    public static Texture2D BlankTexture(this SpriteBatch s)
    {
        var texture = new Texture2D(s.GraphicsDevice, 1, 1);
        texture.SetData(new[] { Color.White });
        return texture;
    }
 
}
