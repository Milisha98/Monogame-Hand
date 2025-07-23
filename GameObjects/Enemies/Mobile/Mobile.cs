using Hands.Core;
using Hands.Core.Animation;
using Hands.Core.Managers.Collision;
using Hands.Core.Managers.Explosion;
using Hands.Core.Managers.Smoke;
using Hands.Core.Sprites;
using Hands.GameObjects.Projectiles;


namespace Hands.GameObjects.Enemies.Mobile;
internal class Mobile : IUpdate, IDraw, IMapPosition, ISleep, ICollision
{
    private readonly MobileInfo _info;
    private readonly Tween _fireDelay;

    private int _animationFrame = 0;
    private float _animationRotation = 0f;

    public Mobile(MobileInfo info)
    {
        MapPosition = new Vector2(info.X, info.Y);
        _info = info;
        WakeDistance = _info.WakeDistance <= 0f ? Global.World.GlobalWakeDistance : _info.WakeDistance;

        _fireDelay = new Tween(TimeSpan.FromSeconds(info.RoF));
    }

    #region IUpdate

    public void Update(GameTime gameTime)
    {
        if (State == MobileState.Destroyed) return;
        if (State == MobileState.Asleep) return;

        float pct = _fireDelay.Update(gameTime);

        // Turn the turret to face the player
        Vector2 playerPosition = Global.World.Player.MapPosition;
        Vector2 direction = playerPosition - Center;
        _animationRotation = MathF.Atan2(direction.Y, direction.X);

        // Update the animation frame based on the fire delay
        _animationFrame = (int)MathF.Min(3, MathF.Floor(pct * 4));

        if (State == MobileState.Active && _fireDelay.IsComplete)
        {
            Shoot(direction);
        }
    }

    #endregion

    #region Shoot

    public float ShootVelocity => 5f;
    public void Shoot(Vector2 direction)
    {
        var firePosition = Center + Vector2.Normalize(direction) * 20f;
        var fireVector = direction;
        if (fireVector != Vector2.Zero) fireVector.Normalize();
        fireVector *= ShootVelocity;
        var projectile = new ProjectileInfo(ProjectileType.BlueBall, firePosition, fireVector, 1f, CollisionType.ProjectileEnemy);
        Global.World.ProjectileManager.Register(projectile);
        _fireDelay.Reset();
    }
    #endregion

    #region IDraw
    public void Draw(SpriteBatch spriteBatch)
    {
        if (State.In(MobileState.Asleep, MobileState.Active))
        {
            DrawActive(spriteBatch);
        }
        else if (State == MobileState.Destroyed)
        {
            DrawDestroyed(spriteBatch);
        }
    }

    private void DrawActive(SpriteBatch spriteBatch)
    {
        var frame = Sprite.Frames[_animationFrame].SourceRectangle;
        spriteBatch.Draw(Sprite.Texture, MapPosition, frame, Color.White, _animationRotation, Size40.Center, 1f, SpriteEffects.None, 0);
        // TODO: Draw Shadow?
    }

    private void DrawDestroyed(SpriteBatch spriteBatch)
    {
        var frame = Sprite.Frames[0].SourceRectangle;
        spriteBatch.Draw(Sprite.Texture, MapPosition, frame, Color.Gray, _animationRotation, Size40.Center, 1f, SpriteEffects.None, 0);
    }

    internal MobileSprite Sprite => Manager.Sprite;

    #endregion

    #region ISleep

    public float WakeDistance { get; init; }
    public bool IsAsleep { get; private set; } = true;

    public void OnSisterAwake()
    {
        if (State == MobileState.Destroyed) return;
        State = MobileState.Active;
        IsAsleep = false;
        _fireDelay.IsActive = true;

        Global.World.CollisionManager.Register(this);
    }

    public void OnSleep()
    {
        if (State == MobileState.Destroyed) return;
        State = MobileState.Asleep;

        _fireDelay.Reset();
        _fireDelay.IsActive = false;

        Global.World.CollisionManager.UnRegister(this);
    }

    #endregion

    #region ICollision

    public void OnCollide(ICollision other)
    {
        if (State == MobileState.Destroyed) return;

        _fireDelay.IsActive = false;
        State = MobileState.Destroyed;

        Global.World.CollisionManager.UnRegister(this);

        Explode();
        Smoke();
    }

    private void Explode()
    {
        var explosionInfo = new ExplosionInfo(Center, 40);
        Global.World.ExplosionManager.Register(explosionInfo);
    }

    private void Smoke()
    {
        var smokeRadius = 40f * 1.5f; // Increase radius for better coverage
        var smokeParticleCount = (int)(40 * 1.5f); // Scale particles with radius
        var smokeInfo = new SmokeAreaInfo(Center, smokeRadius, smokeParticleCount, 0.1f);
        Global.World.SmokeManager.Register(smokeInfo);
    }

    public Rectangle Clayton => new Rectangle(MapPosition.ToPoint(), Size40.Point);

    public Rectangle[] CollisionRectangles => [Clayton];

    public CollisionType CollisionType => CollisionType.Mobile;

    public bool IsHot => true;

    public bool ShouldRemoveOnCollision => State == MobileState.Destroyed; // Only remove when destroyed

    #endregion

    #region IMapPosition

    public Vector2 MapPosition { get; private set; }
    public Vector2 Center => MapPosition + Size40.Center;

    #endregion

    public MobileState State { get; private set; } = MobileState.Asleep;

    public float MovementSpeed => _info.MovementSpeed;

    public MobileManager Manager => Global.World.MobileManager;

}
