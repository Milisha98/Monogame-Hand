using Hands.Core;
using Hands.Core.Sprites;

namespace Hands.GameObjects.Enemies.Boss;
public class Key : IDraw, IMapPosition
{
    private readonly KeyInfo _keyInfo;
    public Boss Boss => Global.World.Boss;

    public Key(KeyInfo info)
    {
        _keyInfo = info;
        MapPosition = new Vector2(info.X, info.Y);
    }

    #region IDraw

    public void Draw(SpriteBatch spriteBatch)
    {
        int w = Size12.Point.X;

        DrawKeyLeft(spriteBatch);
        DrawSpacers(spriteBatch, w);
        DrawModSpacer(spriteBatch, w);
        DrawKeyRight(spriteBatch, w);
    }

    private void DrawKeyLeft(SpriteBatch spriteBatch)
    {
        // Always draw the left
        spriteBatch.Draw(Boss.Sprite.Texture, MapPosition, Boss.Sprite.Frames[0].SourceRectangle, Color.White);
        spriteBatch.Draw(Boss.Sprite.Texture, MapPosition + Size12.Height, Boss.Sprite.Frames[3].SourceRectangle, Color.White);
        spriteBatch.Draw(Boss.Sprite.Texture, MapPosition + Size12.Height + Size12.Height, Boss.Sprite.Frames[3].SourceRectangle, Color.White);
        spriteBatch.Draw(Boss.Sprite.Texture, MapPosition + Size12.Height + Size12.Height + Size12.Height, Boss.Sprite.Frames[5].SourceRectangle, Color.White);
    }


    private void DrawSpacers(SpriteBatch spriteBatch, int w)
    {
        // Draw Spacers
        for (int i = 0; i < SpacerFrames; i++)
        {
            float x = MapPosition.X + w + (i * w);
            Vector2 pos = new(x, MapPosition.Y);
            spriteBatch.Draw(Boss.Sprite.Texture, pos, Boss.Sprite.Frames[1].SourceRectangle, Color.White);
            spriteBatch.Draw(Boss.Sprite.Texture, pos + Size12.Height, Boss.Sprite.Frames[4].SourceRectangle, Color.White);
            spriteBatch.Draw(Boss.Sprite.Texture, pos + Size12.Height + Size12.Height, Boss.Sprite.Frames[4].SourceRectangle, Color.White);
            spriteBatch.Draw(Boss.Sprite.Texture, pos + Size12.Height + Size12.Height + Size12.Height, Boss.Sprite.Frames[6].SourceRectangle, Color.White);
        }
    }

    private void DrawModSpacer(SpriteBatch spriteBatch, int w)
    {
        // Draw Mod Spacer if needed
        if (SpacerModWidth > 0)
        {
            float x = MapPosition.X + w + (SpacerFrames * w);
            Vector2 pos = new(x, MapPosition.Y);
            Rectangle sourceRect = Boss.Sprite.Frames[1].SourceRectangle;
            sourceRect.Width = SpacerModWidth;
            spriteBatch.Draw(Boss.Sprite.Texture, pos, sourceRect, Color.White);

            sourceRect = Boss.Sprite.Frames[4].SourceRectangle;
            sourceRect.Width = SpacerModWidth;
            spriteBatch.Draw(Boss.Sprite.Texture, pos + Size12.Height, sourceRect, Color.White);
            spriteBatch.Draw(Boss.Sprite.Texture, pos + Size12.Height + Size12.Height, sourceRect, Color.White);

            sourceRect = Boss.Sprite.Frames[6].SourceRectangle;
            sourceRect.Width = SpacerModWidth;
            spriteBatch.Draw(Boss.Sprite.Texture, pos + Size12.Height + Size12.Height + Size12.Height, sourceRect, Color.White);
        }
    }

    private void DrawKeyRight(SpriteBatch spriteBatch, int w)
    {
        // Always draw the right
        float rx = MapPosition.X + _keyInfo.Width - w;
        Vector2 rpos = new(rx, MapPosition.Y);
        spriteBatch.Draw(Boss.Sprite.Texture, rpos, Boss.Sprite.Frames[2].SourceRectangle, Color.White);
        spriteBatch.Draw(Boss.Sprite.Texture, MapPosition + Size12.Height, Boss.Sprite.Frames[8].SourceRectangle, Color.White);
        spriteBatch.Draw(Boss.Sprite.Texture, MapPosition + Size12.Height + Size12.Height, Boss.Sprite.Frames[8].SourceRectangle, Color.White);
        spriteBatch.Draw(Boss.Sprite.Texture, MapPosition + Size12.Height + Size12.Height + Size12.Height, Boss.Sprite.Frames[7].SourceRectangle, Color.White);
    }

    #endregion

    #region KeyInfo Properties

    public int SpacerFrames => _keyInfo.Width / Size12.Point.X;
    public int SpacerModWidth => _keyInfo.Width % Size12.Point.X;

    #endregion

    #region IMapPosition

    public Vector2 MapPosition { get; private set; }
    public Vector2 Center => MapPosition + Size12.Center;

    #endregion

}
