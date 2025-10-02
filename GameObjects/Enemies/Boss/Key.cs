using Hands.Core;
using Hands.Core.Sprites;
using Hands.Core.Managers.Collision;
using Hands.Core.Animation;
using Hands.GameObjects.Projectiles;

namespace Hands.GameObjects.Enemies.Boss;
public class Key : IDraw, IMapPosition, ICollision, IUpdate
{
    private readonly KeyInfo _keyInfo;
    public Boss Boss => Global.World.Boss;
    
    // Callback for when this key is interrupted (shot by player)
    public Action OnInterrupted { get; set; }
    
    /// <summary>
    /// The current tint color for rendering the key (includes glow effect)
    /// </summary>
    public Color TintColor { get; private set; } = Color.White;

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
        DrawHorizontalSpacers(spriteBatch);
        DrawHorizontalModSpacer(spriteBatch, w);
        DrawKeyRight(spriteBatch, w);
        
        // Draw key text labels
        DrawKeyText(spriteBatch);
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
        spriteBatch.Draw(Boss.Sprite.Texture, pos, Boss.Sprite.Frames[0].SourceRectangle, TintColor);

        for (int i = 0; i < SpacerFramesY; i++)
        {
            pos = MapPosition + Size12.Height + (Size12.Height * i);
            spriteBatch.Draw(Boss.Sprite.Texture, pos, Boss.Sprite.Frames[4].SourceRectangle, TintColor);
        }

        pos += Size12.Height;
        spriteBatch.Draw(Boss.Sprite.Texture, pos, Boss.Sprite.Frames[8].SourceRectangle, TintColor);
    }


    private void DrawHorizontalSpacers(SpriteBatch spriteBatch)
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

                spriteBatch.Draw(Boss.Sprite.Texture, pos, Boss.Sprite.Frames[frame].SourceRectangle, TintColor);
                pos += Size12.Height;
            }
        }

    }

    private void DrawHorizontalModSpacer(SpriteBatch spriteBatch, int w)
    {
        // Draw Mod Spacer if needed
        if (SpacerModWidth > 0)
        {
            float x = MapPosition.X + w + (SpacerFramesX * w);
            Vector2 pos = new(x, MapPosition.Y);
            Rectangle sourceRect = Boss.Sprite.Frames[1].SourceRectangle;
            sourceRect.Width = SpacerModWidth;
            spriteBatch.Draw(Boss.Sprite.Texture, pos, sourceRect, TintColor);

            for (int y = 0; y < SpacerFramesY; y++)
            {
                pos += Size12.Height;
                sourceRect = Boss.Sprite.Frames[5].SourceRectangle;
                sourceRect.Width = SpacerModWidth;
                spriteBatch.Draw(Boss.Sprite.Texture, pos, sourceRect, TintColor);

            }

            sourceRect = Boss.Sprite.Frames[9].SourceRectangle;
            sourceRect.Width = SpacerModWidth;
            pos += Size12.Height;
            spriteBatch.Draw(Boss.Sprite.Texture, pos, sourceRect, TintColor);
        }
    }

    private void DrawKeyRight(SpriteBatch spriteBatch, int w)
    {
        // Always draw the left
        float rx = MapPosition.X + _keyInfo.Width - w;
        Vector2 pos = new(rx, MapPosition.Y);
        spriteBatch.Draw(Boss.Sprite.Texture, pos, Boss.Sprite.Frames[2].SourceRectangle, TintColor);

        for (int i = 0; i < SpacerFramesY; i++)
        {
            pos += Size12.Height;
            spriteBatch.Draw(Boss.Sprite.Texture, pos, Boss.Sprite.Frames[6].SourceRectangle, TintColor);
        }

        pos += Size12.Height;
        spriteBatch.Draw(Boss.Sprite.Texture, pos, Boss.Sprite.Frames[10].SourceRectangle, TintColor);

    }
    
    private void DrawKeyText(SpriteBatch spriteBatch)
    {
        // Don't draw text if there's no font loaded or no text to draw
        if (Boss.KeyFont == null || (string.IsNullOrEmpty(_keyInfo.Key1) && string.IsNullOrEmpty(_keyInfo.Key2)))
            return;
            
        const int marginTopBottom = 2; // 2 pixel margin from top and bottom edges
        Vector2 keyCenter = MapPosition + new Vector2(_keyInfo.Width / 2.0f, _keyInfo.Height / 2.0f);
        
        // If Key2 is empty/null, center Key1 in the middle of the key
        if (string.IsNullOrEmpty(_keyInfo.Key2))
        {
            if (!string.IsNullOrEmpty(_keyInfo.Key1))
            {
                Vector2 textSize = Boss.KeyFont.MeasureString(_keyInfo.Key1);
                Vector2 textPosition = keyCenter - (textSize / 2);
                spriteBatch.DrawString(Boss.KeyFont, _keyInfo.Key1, textPosition, Color.WhiteSmoke);
            }
        }
        else
        {
            // Calculate available height for text positioning (excluding margins)
            float availableHeight = _keyInfo.Height - (marginTopBottom * 2);
            
            // Draw Key1 at the top center of the key (with margin)
            if (!string.IsNullOrEmpty(_keyInfo.Key1))
            {
                Vector2 key1Size = Boss.KeyFont.MeasureString(_keyInfo.Key1);
                Vector2 key1Position = new Vector2(
                    keyCenter.X - (key1Size.X / 2),
                    MapPosition.Y + marginTopBottom + (availableHeight * 0.25f) - (key1Size.Y / 2)
                );
                spriteBatch.DrawString(Boss.KeyFont, _keyInfo.Key1, key1Position, Color.WhiteSmoke);
            }
            
            // Draw Key2 at the bottom center of the key (with margin)
            Vector2 key2Size = Boss.KeyFont.MeasureString(_keyInfo.Key2);
            Vector2 key2Position = new Vector2(
                keyCenter.X - (key2Size.X / 2),
                MapPosition.Y - marginTopBottom + (availableHeight * 0.75f) - (key2Size.Y / 2)
            );
            spriteBatch.DrawString(Boss.KeyFont, _keyInfo.Key2, key2Position, Color.WhiteSmoke);
        }
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

    public bool IsHot { get; set; } = false; // Keys are static collision objects

    public bool ShouldRemoveOnCollision => false; // Keys should not be removed when hit

    public void OnCollide(ICollision other)
    {
        // Only react to player projectiles when glowing (can be interrupted)
        if (other.CollisionType == CollisionType.ProjectilePlayer && GlowIntensity > 0)
        {
            // Set intensity to 0 immediately
            GlowIntensity = 0f;
            
            // Notify the phase that this key was interrupted
            OnInterrupted?.Invoke();
            
            // The collision system will handle destroying the projectile
        }
        // Keys are otherwise solid barriers that don't react to collisions
    }

    #endregion
    
    #region IUpdate
    
    public void Update(GameTime gameTime)
    {
        // Update tint color based on glow intensity
        TintColor = GlowIntensity > 0 ? Color.Lerp(Color.White, Color.Red, GlowIntensity) : Color.White;
    }
    
    #endregion
    
    
    /// <summary>
    /// Gets or sets the glow intensity (0.0 to 1.0)
    /// </summary>
    public float GlowIntensity { get; set; } = 0f;
    
    
    #region Shooting
    
    /// <summary>
    /// Shoots a fan of bullets downward from the center of the key
    /// </summary>
    public void ShootBulletFan()
    {
        const int bulletCount = 10;
        const float fanAngleDegrees = 15f; // 15 degrees between each bullet
        const float totalFanAngle = (bulletCount - 1) * fanAngleDegrees * MathF.PI / 180f; // Convert to radians
        const float startAngle = MathF.PI / 2f - totalFanAngle / 2f; // Start angle for the fan (pointing down)
        const float bulletSpeed = 3f;
        
        Vector2 shootPosition = MapPosition + new Vector2(_keyInfo.Width / 2f, _keyInfo.Height); // Bottom center of key
        
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + (i * fanAngleDegrees * MathF.PI / 180f);
            Vector2 direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            Vector2 velocity = direction * bulletSpeed;
            
            var projectileInfo = new ProjectileInfo(
                ProjectileType.RedBall, // Use red bullets for boss
                shootPosition,
                velocity,
                1f, // Scale
                CollisionType.ProjectileEnemy
            );
            
            Global.World.ProjectileManager.Register(projectileInfo);
        }
    }
    
    #endregion

}
