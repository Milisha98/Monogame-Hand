using Hands.Core;
using Hands.Core.Sprites;
using Hands.Core.Managers.Collision;

namespace Hands.GameObjects.Enemies.Boss;
public class Key : IDraw, IMapPosition, ICollision
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
        DrawSpacers(spriteBatch);
        DrawModSpacer(spriteBatch, w);
        DrawKeyRight(spriteBatch, w);
    }

    public void DrawShadow(SpriteBatch spriteBatch)
    {
        DrawBottomShadows(spriteBatch);
        DrawRightShadows(spriteBatch);
    }

    private void DrawRightShadows(SpriteBatch spriteBatch)
    {
        var right = new Vector2(_keyInfo.Width, 0);
        var offset = new Vector2(0, 12);
        var pos = MapPosition + right;
        spriteBatch.Draw(Boss.Sprite.Texture, pos, Boss.Sprite.Frames[3].SourceRectangle, Color.White);

        for (int y = 0; y < SpacerFramesY; y++)
        {
            pos += offset;
            spriteBatch.Draw(Boss.Sprite.Texture, pos, Boss.Sprite.Frames[7].SourceRectangle, Color.White);
        }
        
        pos += offset;
        spriteBatch.Draw(Boss.Sprite.Texture, pos, Boss.Sprite.Frames[13].SourceRectangle, Color.White);

    }

    private void DrawBottomShadows(SpriteBatch spriteBatch)
    {
        var bottom = new Vector2(0, _keyInfo.Height);
        var offset = new Vector2(12, 0);

        // Left Shadow
        spriteBatch.Draw(Boss.Sprite.Texture, MapPosition + bottom, Boss.Sprite.Frames[11].SourceRectangle, Color.White);

        // Spacer Shadows
        var pos = bottom;
        for (int i = 0; i < SpacerFramesX; i++)
        {
            pos = bottom + offset + (offset * i);
            spriteBatch.Draw(Boss.Sprite.Texture, MapPosition + pos, Boss.Sprite.Frames[12].SourceRectangle, Color.White);
        }

        // Mod Spacer Shadow
        pos = bottom + offset + (offset * SpacerFramesX);
        if (SpacerModWidth > 0)
        {
            Rectangle sourceRect = Boss.Sprite.Frames[12].SourceRectangle;
            sourceRect.Width = SpacerModWidth;
            spriteBatch.Draw(Boss.Sprite.Texture, MapPosition + pos, sourceRect, Color.White);
        }

        // Right Shadow
        pos = new Vector2(_keyInfo.Width - 12, 0) + bottom;
        spriteBatch.Draw(Boss.Sprite.Texture, MapPosition + pos, Boss.Sprite.Frames[12].SourceRectangle, Color.White);
    }

    private void DrawKeyLeft(SpriteBatch spriteBatch)
    {
        // Always draw the left
        var pos = MapPosition;
        spriteBatch.Draw(Boss.Sprite.Texture, pos, Boss.Sprite.Frames[0].SourceRectangle, Color.White);

        for (int i = 0; i < SpacerFramesY; i++)
        {
            pos = MapPosition + Size12.Height + (Size12.Height * i);
            spriteBatch.Draw(Boss.Sprite.Texture, pos, Boss.Sprite.Frames[4].SourceRectangle, Color.White);
        }

        pos += Size12.Height;
        spriteBatch.Draw(Boss.Sprite.Texture, pos, Boss.Sprite.Frames[8].SourceRectangle, Color.White);
    }


    private void DrawSpacers(SpriteBatch spriteBatch)
    {
        // Draw Spacers
        Vector2 pos = MapPosition + Size12.Width;
        for (int x = 0; x < SpacerFramesX; x++)
        {
            pos = MapPosition + Size12.Width + (Size12.Width * x);
            for (int y = 0; y < SpacerFramesY + 2; y++)
            {
                int frame = y switch
                {
                    0 => 1,
                    _ => (y == SpacerFramesY + 1) ? 9 : 5,
                };

                spriteBatch.Draw(Boss.Sprite.Texture, pos, Boss.Sprite.Frames[frame].SourceRectangle, Color.White);
                pos += Size12.Height;
            }
        }

    }

    private void DrawModSpacer(SpriteBatch spriteBatch, int w)
    {
        // Draw Mod Spacer if needed
        if (SpacerModWidth > 0)
        {
            float x = MapPosition.X + w + (SpacerFramesX * w);
            Vector2 pos = new(x, MapPosition.Y);
            Rectangle sourceRect = Boss.Sprite.Frames[1].SourceRectangle;
            sourceRect.Width = SpacerModWidth;
            spriteBatch.Draw(Boss.Sprite.Texture, pos, sourceRect, Color.White);

            for (int y = 0; y < SpacerFramesY; y++)
            {
                pos += Size12.Height;
                sourceRect = Boss.Sprite.Frames[5].SourceRectangle;
                sourceRect.Width = SpacerModWidth;
                spriteBatch.Draw(Boss.Sprite.Texture, pos, sourceRect, Color.White);

            }

            sourceRect = Boss.Sprite.Frames[9].SourceRectangle;
            sourceRect.Width = SpacerModWidth;
            pos += Size12.Height;
            spriteBatch.Draw(Boss.Sprite.Texture, pos, sourceRect, Color.White);
        }
    }

    private void DrawKeyRight(SpriteBatch spriteBatch, int w)
    {
        // Always draw the left
        float rx = MapPosition.X + _keyInfo.Width - w;
        Vector2 pos = new(rx, MapPosition.Y);
        spriteBatch.Draw(Boss.Sprite.Texture, pos, Boss.Sprite.Frames[2].SourceRectangle, Color.White);

        for (int i = 0; i < SpacerFramesY; i++)
        {
            pos += Size12.Height;
            spriteBatch.Draw(Boss.Sprite.Texture, pos, Boss.Sprite.Frames[6].SourceRectangle, Color.White);
        }

        pos += Size12.Height;
        spriteBatch.Draw(Boss.Sprite.Texture, pos, Boss.Sprite.Frames[10].SourceRectangle, Color.White);

    }

    #endregion

    #region KeyInfo Properties

    public int SpacerFramesX => (_keyInfo.Width / Size12.Point.X) - 2; // Exclude left & right
    public int SpacerFramesY => (_keyInfo.Height / Size12.Point.Y) - 2; // Exclude top & bottom
    public int SpacerModWidth => _keyInfo.Width % Size12.Point.X;

    #endregion

    #region IMapPosition

    public Vector2 MapPosition { get; private set; }
    public Vector2 Center => MapPosition + Size12.Center;

    #endregion

    #region ICollision

    public Rectangle Clayton => new Rectangle(MapPosition.ToPoint(), new Point(_keyInfo.Width, 48));

    public Rectangle[] CollisionRectangles => [Clayton];

    public CollisionType CollisionType => CollisionType.Turret;

    public bool IsHot { get; set; } = true; // Keys are always active for collision

    public bool ShouldRemoveOnCollision => false; // Keys should not be removed when hit

    public void OnCollide(ICollision other)
    {
        // Keys don't react to collisions - they are solid barriers
        // The collision system handles the response for the other object
    }

    #endregion

}
