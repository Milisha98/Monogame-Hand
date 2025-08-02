using Hands.Core;
using Hands.Core.Animation;
using Hands.Core.Managers.Collision;
using Hands.Core.Managers.Explosion;
using Hands.Core.Managers.Smoke;
using Hands.Core.Sprites;
using Hands.GameObjects.Projectiles;

namespace Hands.GameObjects.Enemies.SideGun;

internal class SideGun : IUpdate, IDraw, IMapPosition, ISleep, ICollision
{
    // Horizontal orientation constants
    private const int GunXOffset = 15; // X offset from base position for horizontal orientations
    private const int TopGunYOffset = -3; // Y offset for top gun (horizontal orientations)
    private const int BottomGunYOffset = 13; // Y offset for bottom gun (horizontal orientations)
    
    // Vertical orientation constants
    private const int GunYOffset = 15; // Y offset from base position for vertical orientations
    private const int LeftGunXOffset = -3; // X offset for left gun (vertical orientations)
    private const int RightGunXOffset = 13; // X offset for right gun (vertical orientations)
    
    private readonly SideGunInfo _info;

    private SinTween _gun1SinTween; // Top gun for horizontal, Right gun for vertical
    private SinTween _gun2SinTween; // Bottom gun for horizontal, Left gun for vertical
    private float _gun1AnimationOffset = 8f; // Initialize to center position (sin=0 maps to offset=8)
    private float _gun2AnimationOffset = 8f; // Initialize to center position (sin=0 maps to offset=8)

    public SideGun(SideGunInfo info)
    {
        MapPosition = new Vector2(info.X, info.Y);
        Orientation = info.Orientation;
        _info = info;
        _gun1SinTween = new SinTween(TimeSpan.FromSeconds(2)); // Full 2-second sin cycle
        _gun2SinTween = new SinTween(TimeSpan.FromSeconds(2)); // Full 2-second sin cycle
        
        // Wire up the events to shoot
        _gun1SinTween.OnTroughReached += () => Shoot(true); // Gun1 fires at trough (max extension)
        _gun2SinTween.OnPeakReached += () => Shoot(false); // Gun2 fires at peak (max extension due to inversion)

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
            float gun1Sin = _gun1SinTween.Update(gameTime);
            float gun2Sin = _gun2SinTween.Update(gameTime);
            
            // Convert sin values to animation offsets (0 to 16 pixels)
            // Sin values range from -1 to 1, we want 0 to 16
            _gun1AnimationOffset = (gun1Sin + 1) * 8f; // Maps -1..1 to 0..16
            _gun2AnimationOffset = (-gun2Sin + 1) * 8f; // Inverted gun2 for π phase offset
        }
        // Note: When inactive, preserve the last position by not updating animation offsets
    }

    #endregion

    #region IDraw

    public void Draw(SpriteBatch spriteBatch)
    {
        switch (State)
        {
            case SideGunState.Idle:
                DrawActive(spriteBatch);
                break;
            case SideGunState.Active:
                DrawActive(spriteBatch);
                break;
            case SideGunState.Destroyed:
                DrawDestroyed(spriteBatch);
                break;
        }

    }

    private void DrawActive(SpriteBatch spriteBatch)
    {
        var spriteEffects = SpriteEffects.None;
        int baseFrame, gunFrame;
        float baseXPosition, topGunXPosition, bottomGunXPosition;
        Vector2 gunPositionTop, gunPositionBottom;

        switch (Orientation)
        {
            case SideGunOrientation.Left:
                spriteEffects = SpriteEffects.FlipHorizontally;
                baseFrame = 0;
                gunFrame = 1;
                baseXPosition = MapPosition.X + GunXOffset;
                topGunXPosition = baseXPosition - _gun1AnimationOffset;
                bottomGunXPosition = baseXPosition - _gun2AnimationOffset;
                gunPositionTop = new Vector2(topGunXPosition, MapPosition.Y + TopGunYOffset);
                gunPositionBottom = new Vector2(bottomGunXPosition, MapPosition.Y + BottomGunYOffset);
                break;
            case SideGunOrientation.Right:
                baseFrame = 0;
                gunFrame = 1;
                baseXPosition = MapPosition.X - GunXOffset;
                topGunXPosition = baseXPosition + _gun1AnimationOffset;
                bottomGunXPosition = baseXPosition + _gun2AnimationOffset;
                gunPositionTop = new Vector2(topGunXPosition, MapPosition.Y + TopGunYOffset);
                gunPositionBottom = new Vector2(bottomGunXPosition, MapPosition.Y + BottomGunYOffset);
                break;
            case SideGunOrientation.Up:
                baseFrame = 2;
                gunFrame = 3;
                baseXPosition = MapPosition.X;
                topGunXPosition = baseXPosition + RightGunXOffset;
                bottomGunXPosition = baseXPosition + LeftGunXOffset;
                gunPositionTop = new Vector2(topGunXPosition, MapPosition.Y - _gun1AnimationOffset);
                gunPositionBottom = new Vector2(bottomGunXPosition, MapPosition.Y - _gun2AnimationOffset);
                break;
            case SideGunOrientation.Down:
                spriteEffects = SpriteEffects.FlipVertically;
                baseFrame = 2;
                gunFrame = 3;
                baseXPosition = MapPosition.X;
                topGunXPosition = baseXPosition + RightGunXOffset;
                bottomGunXPosition = baseXPosition + LeftGunXOffset;
                gunPositionTop = new Vector2(topGunXPosition, MapPosition.Y + _gun1AnimationOffset);
                gunPositionBottom = new Vector2(bottomGunXPosition, MapPosition.Y + _gun2AnimationOffset);
                break;
            default:
                return;
        }

        // Draw the gun sprites
        spriteBatch.Draw(Sprite.Texture, gunPositionTop, Sprite.Frames[gunFrame].SourceRectangle, Color.White, 0f, Vector2.Zero, 1f, spriteEffects, 0f);
        spriteBatch.Draw(Sprite.Texture, gunPositionBottom, Sprite.Frames[gunFrame].SourceRectangle, Color.White, 0f, Vector2.Zero, 1f, spriteEffects, 0f);

        // Draw the base mount sprite
        spriteBatch.Draw(Sprite.Texture, MapPosition, Sprite.Frames[baseFrame].SourceRectangle, Color.White, 0f, Vector2.Zero, 1f, spriteEffects, 0f);
    }

    private void DrawDestroyed(SpriteBatch spriteBatch)
    {
        var destroyedColor = Color.Gray;
        var spriteEffects = SpriteEffects.None;
        int baseFrame;

        switch (Orientation)
        {
            case SideGunOrientation.Left:
                spriteEffects = SpriteEffects.FlipHorizontally;
                baseFrame = 0;
                break;
            case SideGunOrientation.Right:
                baseFrame = 0;
                break;
            case SideGunOrientation.Up:
                baseFrame = 2;
                break;
            case SideGunOrientation.Down:
                spriteEffects = SpriteEffects.FlipVertically;
                baseFrame = 2;
                break;
            default:
                baseFrame = 0;
                break;
        }

        // Draw the base mount sprite
        spriteBatch.Draw(Sprite.Texture, MapPosition, Sprite.Frames[baseFrame].SourceRectangle, destroyedColor, 0f, Vector2.Zero, 1f, spriteEffects, 0f);
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
        _gun1SinTween.IsActive = true;
        _gun2SinTween.IsActive = true;
        
        Global.World.CollisionManager.Register(this);
    }

    public void OnSleep()
    {
        if (State == SideGunState.Destroyed) return;
        State = SideGunState.Idle;
        IsAsleep = true;
        
        // Disable the sin tweens when going to sleep (preserves their state)
        _gun1SinTween.IsActive = false;
        _gun2SinTween.IsActive = false;
        
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
    public bool ShouldRemoveOnCollision => State == SideGunState.Destroyed; // Only remove when destroyed

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

    private void Shoot(bool isGun1)
    {
        const int GunLength = 16;
        const int ShootVelocity = 5;
        const int GunBarrelOffset = 4; // Offset from gun position to barrel tip

        Vector2 firePosition;
        Vector2 fireVector;

        switch (Orientation)
        {
            case SideGunOrientation.Left:
                {
                    float baseXPosition = MapPosition.X + GunXOffset + GunLength;
                    if (isGun1) // Top gun
                    {
                        float gunXPosition = baseXPosition - _gun1AnimationOffset;
                        firePosition = new Vector2(gunXPosition, MapPosition.Y + TopGunYOffset + GunBarrelOffset);
                    }
                    else // Bottom gun
                    {
                        float gunXPosition = baseXPosition - _gun2AnimationOffset;
                        firePosition = new Vector2(gunXPosition, MapPosition.Y + BottomGunYOffset + GunBarrelOffset);
                    }
                    fireVector = new Vector2(ShootVelocity, 0);
                }
                break;
            case SideGunOrientation.Right:
                {
                    float baseXPosition = MapPosition.X - GunXOffset - GunLength;
                    if (isGun1) // Top gun
                    {
                        float gunXPosition = baseXPosition + _gun1AnimationOffset;
                        firePosition = new Vector2(gunXPosition, MapPosition.Y + TopGunYOffset + GunBarrelOffset);
                    }
                    else // Bottom gun
                    {
                        float gunXPosition = baseXPosition + _gun2AnimationOffset;
                        firePosition = new Vector2(gunXPosition, MapPosition.Y + BottomGunYOffset + GunBarrelOffset);
                    }
                    fireVector = new Vector2(-ShootVelocity, 0);
                }
                break;
            case SideGunOrientation.Up:
                {
                    float baseYPosition = MapPosition.Y - GunYOffset - GunLength;
                    if (isGun1) // Right gun
                    {
                        float gunYPosition = baseYPosition - _gun1AnimationOffset;
                        firePosition = new Vector2(MapPosition.X + RightGunXOffset + GunBarrelOffset, gunYPosition);
                    }
                    else // Left gun
                    {
                        float gunYPosition = baseYPosition - _gun2AnimationOffset;
                        firePosition = new Vector2(MapPosition.X + LeftGunXOffset + GunBarrelOffset, gunYPosition);
                    }
                    fireVector = new Vector2(0, -ShootVelocity);
                }
                break;
            case SideGunOrientation.Down:
                {
                    float baseYPosition = MapPosition.Y + GunYOffset + GunLength;
                    if (isGun1) // Right gun
                    {
                        float gunYPosition = baseYPosition + _gun1AnimationOffset;
                        firePosition = new Vector2(MapPosition.X + RightGunXOffset + GunBarrelOffset, gunYPosition);
                    }
                    else // Left gun
                    {
                        float gunYPosition = baseYPosition + _gun2AnimationOffset;
                        firePosition = new Vector2(MapPosition.X + LeftGunXOffset + GunBarrelOffset, gunYPosition);
                    }
                    fireVector = new Vector2(0, ShootVelocity);
                }
                break;
            default:
                return; // Invalid orientation
        }

        var projectile = new ProjectileInfo(ProjectileType.BlueBall, firePosition, fireVector, 1f, CollisionType.ProjectileEnemy);
        Global.World.ProjectileManager.Register(projectile);
    }

    // Properties
    public string ID => _info.ID;
    public SideGunState State { get; private set; } = SideGunState.Idle;
    public SideGunOrientation Orientation { get; private set; }
    internal SideGunManager Manager => Global.World.SideGunManager;
    internal SideGunSprite Sprite => Manager.Sprite;
}
