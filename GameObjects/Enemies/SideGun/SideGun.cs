using Hands.Core;
using Hands.Core.Animation;
using Hands.Core.Managers.Collision;
using Hands.Core.Managers.Explosion;
using Hands.Core.Managers.Smoke;
using Hands.Core.Sprites;
using Hands.Sprites;
using Microsoft.Xna.Framework.Graphics;

namespace Hands.GameObjects.Enemies.SideGun;

internal class SideGun : IUpdate, IDraw, IMapPosition, ISleep, ICollision
{
    private const int GunXOffset = 15; // X offset from base position (was 16, reduced by 1)
    private const int TopGunYOffset = -3; // Y offset for top gun
    private const int BottomGunYOffset = 13; // Y offset for bottom gun
    
    private readonly SideGunInfo _info;

    private SinTween _topGunSinTween;
    private SinTween _bottomGunSinTween;
    private float _topGunXOffset = 8f; // Initialize to center position (sin=0 maps to offset=8)
    private float _bottomGunXOffset = 8f; // Initialize to center position (sin=0 maps to offset=8)

    public SideGun(SideGunInfo info)
    {
        MapPosition = new Vector2(info.X, info.Y);
        Orientation = info.Orientation;
        _info = info;
        _topGunSinTween = new SinTween(TimeSpan.FromSeconds(1)); // 1-second cycle
        _bottomGunSinTween = new SinTween(TimeSpan.FromSeconds(1)); // 1-second cycle
        
        // Wire up the peak events to shoot
        _topGunSinTween.OnPeakReached += () => Shoot(true); // true = top gun
        _bottomGunSinTween.OnPeakReached += () => Shoot(false); // false = bottom gun

        WakeDistance = _info.WakeDistance <= 0f ? Global.World.GlobalWakeDistance : _info.WakeDistance;
    }

    #region IUpdate

    public void Update(GameTime gameTime)
    {
        if (State == SideGunState.Destroyed) return;
        if (State == SideGunState.Idle) return;

        // Only animate when active
        if (State == SideGunState.Active)
        {
            // Update the sin tweens for gun animation
            float topGunSin = _topGunSinTween.Update(gameTime);
            float bottomGunSin = _bottomGunSinTween.Update(gameTime);
            
            // Convert sin values to X offsets (0 to 16 pixels)
            // Sin values range from -1 to 1, we want 0 to 16
            _topGunXOffset = (topGunSin + 1) * 8f; // Maps -1..1 to 0..16
            _bottomGunXOffset = (-bottomGunSin + 1) * 8f; // Inverted bottom gun for π phase offset
        }
        // Note: When inactive, preserve the last position by not updating _topGunXOffset and _bottomGunXOffset
    }

    #endregion

    #region IDraw

    public void Draw(SpriteBatch spriteBatch)
    {
        switch (State)
        {
            case SideGunState.Idle:
                DrawIdle(spriteBatch);
                break;
            case SideGunState.Active:
                DrawActive(spriteBatch);
                break;
            case SideGunState.Destroyed:
                DrawDestroyed(spriteBatch);
                break;
        }

    }

    private void DrawIdle(SpriteBatch spriteBatch)
    {
        var spriteEffects = Orientation == SideGunOrientation.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        
        // Draw the gun sprite top
        bool isFlipped = Orientation == SideGunOrientation.Left;
        var xPosition = isFlipped ? MapPosition.X + GunXOffset : MapPosition.X - GunXOffset;
        var gunPositionTop = new Vector2(xPosition, MapPosition.Y + TopGunYOffset);
        spriteBatch.Draw(Sprite.Texture, gunPositionTop, Sprite.Frames[1].SourceRectangle, Color.White, 0f, Vector2.Zero, 1f, spriteEffects, 0f);

        // Draw the gun sprite bottom
        var gunPositionBottom = new Vector2(xPosition, MapPosition.Y + BottomGunYOffset);
        spriteBatch.Draw(Sprite.Texture, gunPositionBottom, Sprite.Frames[1].SourceRectangle, Color.White, 0f, Vector2.Zero, 1f, spriteEffects, 0f);

        // Draw the base mount sprite
        spriteBatch.Draw(Sprite.Texture, MapPosition, Sprite.Frames[0].SourceRectangle, Color.White, 0f, Vector2.Zero, 1f, spriteEffects, 0f);

    }

    private void DrawActive(SpriteBatch spriteBatch)
    {
        var spriteEffects = Orientation == SideGunOrientation.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        
        // Calculate gun positions with animated X offsets
        bool isFlipped = Orientation == SideGunOrientation.Left;
        float baseXPosition = isFlipped ? MapPosition.X + GunXOffset : MapPosition.X - GunXOffset;
        
        // Apply the calculated offsets (considering flip direction)
        float topGunXPosition = baseXPosition + (isFlipped ? -_topGunXOffset : _topGunXOffset);
        float bottomGunXPosition = baseXPosition + (isFlipped ? -_bottomGunXOffset : _bottomGunXOffset);
        
        // Draw the gun sprites with animated positions
        var gunPositionTop = new Vector2(topGunXPosition, MapPosition.Y + TopGunYOffset);
        var gunPositionBottom = new Vector2(bottomGunXPosition, MapPosition.Y + BottomGunYOffset);
        
        spriteBatch.Draw(Sprite.Texture, gunPositionTop, Sprite.Frames[1].SourceRectangle, Color.White, 0f, Vector2.Zero, 1f, spriteEffects, 0f);
        spriteBatch.Draw(Sprite.Texture, gunPositionBottom, Sprite.Frames[1].SourceRectangle, Color.White, 0f, Vector2.Zero, 1f, spriteEffects, 0f);


        // Draw the base mount sprite
        spriteBatch.Draw(Sprite.Texture, MapPosition, Sprite.Frames[0].SourceRectangle, Color.White, 0f, Vector2.Zero, 1f, spriteEffects, 0f);
    }

    private void DrawDestroyed(SpriteBatch spriteBatch)
    {
        var spriteEffects = Orientation == SideGunOrientation.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None;        
        var destroyedColor = Color.Gray;

        // Draw the base mount sprite
        spriteBatch.Draw(Sprite.Texture, MapPosition, Sprite.Frames[0].SourceRectangle, destroyedColor, 0f, Vector2.Zero, 1f, spriteEffects, 0f);
    }


    #endregion

    #region ISleep

    public float WakeDistance { get; init; }
    public bool IsAsleep { get; private set; } = true;

    public void OnSisterAwake()
    {
        if (State == SideGunState.Destroyed) return;
        State = SideGunState.Active;
        IsAsleep = false;
        
        // Enable the sin tweens when becoming active
        _topGunSinTween.IsActive = true;
        _bottomGunSinTween.IsActive = true;
        
        Global.World.CollisionManager.Register(this);
    }

    public void OnSleep()
    {
        if (State == SideGunState.Destroyed) return;
        State = SideGunState.Idle;
        IsAsleep = true;
        
        // Disable the sin tweens when going to sleep (preserves their state)
        _topGunSinTween.IsActive = false;
        _bottomGunSinTween.IsActive = false;
        
        Global.World.CollisionManager.UnRegister(this);
    }

    #endregion

    #region ICollision

    public Rectangle Clayton => new Rectangle(MapPosition.ToPoint(), Size32.Point);

    public Rectangle[] CollisionRectangles =>
    [
        new Rectangle((int)MapPosition.X, (int)MapPosition.Y, 32, 32)
    ];

    public CollisionType CollisionType => CollisionType.Turret; // Reuse Turret collision type for now
    public bool IsHot => State == SideGunState.Active;
    public bool ShouldRemoveOnCollision => false; // Don't remove on collision, just destroy

    public void OnCollide(ICollision other)
    {
        if (State != SideGunState.Active) return;

        var explosionInfo = new ExplosionInfo(Center, 32);
        Global.World.ExplosionManager.Register(explosionInfo);

        Global.World.CollisionManager.UnRegister(this);
        State = SideGunState.Destroyed;

        var smokeRadius = 32f * 1.5f;
        var smokeParticleCount = (int)(25 * 1.5f);
        var smokeInfo = new SmokeAreaInfo(Center, smokeRadius, smokeParticleCount, 0.1f);
        Global.World.SmokeManager.Register(smokeInfo);
    }

    #endregion

    #region IMapPosition

    public Vector2 MapPosition { get; private set; }
    public Vector2 Center => MapPosition + Size32.Center;

    #endregion

    private void Shoot(bool isTopGun)
    {
        string gunPosition = isTopGun ? "Top" : "Bottom";
        System.Diagnostics.Debug.WriteLine($"SideGun {ID} {gunPosition} gun fired!");
    }

    // Properties
    public string ID => _info.ID;
    public SideGunState State { get; private set; } = SideGunState.Idle;
    public SideGunOrientation Orientation { get; private set; }
    internal SideGunManager Manager => Global.World.SideGunManager;
    internal SideGunSprite Sprite => Manager.Sprite;
}
