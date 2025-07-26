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

        // Move towards the player when awake
        if (State == MobileState.Active && direction != Vector2.Zero)
        {
            UpdateMovement(gameTime, direction);
        }

        // Update the animation frame based on the fire delay
        _animationFrame = (int)MathF.Min(3, MathF.Floor(pct * 4));

        if (State == MobileState.Active && _fireDelay.IsComplete)
        {
            Shoot(direction);
        }
    }

    private void UpdateMovement(GameTime gameTime, Vector2 direction)
    {
        // Normalize the direction to get movement vector
        var move = direction;
        if (move != Vector2.Zero)
        {
            move.Normalize();
        }
        else
        {
            return; // No movement if no direction
        }

        // Calculate proposed new position
        float speed = MovementSpeed * gameTime.ElapsedGameTime.Milliseconds;
        var proposedMapPosition = MapPosition + (move * speed);
        var proposedCenter = proposedMapPosition + Size40.Center;

        // Use the simplified CheckMoveTo method that returns CollisionType
        var collisionType = Global.World.CollisionManager.CheckMoveTo(proposedCenter, CollisionType.Mobile, Size40.Point);
        if (collisionType is null)
        {
            MapPosition = proposedMapPosition;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"Mobile collision detected at {proposedMapPosition} with {collisionType}");
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

    public Rectangle Clayton => new Rectangle((MapPosition - Size40.Center).ToPoint(), Size40.Point);

    public Rectangle[] CollisionRectangles
    {
        get
        {
            var collisionPoints = new Vector2[]
            {
                new Vector2(8, 0), new Vector2(10, 0), new Vector2(12, 0),
                new Vector2(6, 2), new Vector2(8, 2), new Vector2(10, 2), new Vector2(12, 2), new Vector2(14, 2), new Vector2(16, 2), new Vector2(18, 2), new Vector2(20, 2), new Vector2(22, 2), new Vector2(24, 2), new Vector2(26, 2), new Vector2(28, 2), new Vector2(30, 2),
                new Vector2(4, 4), new Vector2(6, 4), new Vector2(8, 4), new Vector2(10, 4), new Vector2(12, 4), new Vector2(14, 4), new Vector2(16, 4), new Vector2(18, 4), new Vector2(20, 4), new Vector2(22, 4), new Vector2(24, 4), new Vector2(26, 4), new Vector2(28, 4), new Vector2(30, 4),
                new Vector2(2, 6), new Vector2(4, 6), new Vector2(6, 6), new Vector2(8, 6), new Vector2(10, 6), new Vector2(12, 6), new Vector2(14, 6), new Vector2(16, 6), new Vector2(18, 6), new Vector2(20, 6), new Vector2(22, 6), new Vector2(24, 6), new Vector2(26, 6), new Vector2(28, 6), new Vector2(30, 6),
                new Vector2(0, 8), new Vector2(2, 8), new Vector2(4, 8), new Vector2(6, 8), new Vector2(8, 8), new Vector2(10, 8), new Vector2(12, 8), new Vector2(14, 8), new Vector2(16, 8), new Vector2(18, 8), new Vector2(20, 8), new Vector2(22, 8), new Vector2(24, 8), new Vector2(26, 8), new Vector2(28, 8), new Vector2(30, 8),
                new Vector2(0, 10), new Vector2(2, 10), new Vector2(4, 10), new Vector2(6, 10), new Vector2(8, 10), new Vector2(10, 10), new Vector2(12, 10), new Vector2(14, 10), new Vector2(16, 10), new Vector2(18, 10), new Vector2(20, 10), new Vector2(22, 10), new Vector2(24, 10), new Vector2(26, 10), new Vector2(28, 10),
                new Vector2(0, 12), new Vector2(2, 12), new Vector2(4, 12), new Vector2(6, 12), new Vector2(8, 12), new Vector2(10, 12), new Vector2(12, 12), new Vector2(14, 12), new Vector2(16, 12), new Vector2(18, 12), new Vector2(20, 12), new Vector2(22, 12), new Vector2(24, 12), new Vector2(26, 12), new Vector2(28, 12), new Vector2(30, 12), new Vector2(32, 12), new Vector2(34, 12),
                new Vector2(2, 14), new Vector2(4, 14), new Vector2(6, 14), new Vector2(8, 14), new Vector2(10, 14), new Vector2(12, 14), new Vector2(14, 14), new Vector2(16, 14), new Vector2(18, 14), new Vector2(20, 14), new Vector2(22, 14), new Vector2(24, 14), new Vector2(26, 14), new Vector2(28, 14), new Vector2(30, 14), new Vector2(32, 14), new Vector2(34, 14), new Vector2(36, 14),
                new Vector2(4, 16), new Vector2(6, 16), new Vector2(8, 16), new Vector2(10, 16), new Vector2(12, 16), new Vector2(14, 16), new Vector2(16, 16), new Vector2(18, 16), new Vector2(20, 16), new Vector2(22, 16), new Vector2(24, 16), new Vector2(26, 16), new Vector2(28, 16), new Vector2(30, 16), new Vector2(32, 16), new Vector2(34, 16), new Vector2(36, 16),
                new Vector2(4, 18), new Vector2(6, 18), new Vector2(8, 18), new Vector2(10, 18), new Vector2(12, 18), new Vector2(14, 18), new Vector2(16, 18), new Vector2(18, 18), new Vector2(20, 18), new Vector2(22, 18), new Vector2(24, 18), new Vector2(26, 18), new Vector2(28, 18), new Vector2(30, 18), new Vector2(32, 18), new Vector2(34, 18), new Vector2(36, 18), new Vector2(38, 18),
                new Vector2(4, 20), new Vector2(6, 20), new Vector2(8, 20), new Vector2(10, 20), new Vector2(12, 20), new Vector2(14, 20), new Vector2(16, 20), new Vector2(18, 20), new Vector2(20, 20), new Vector2(22, 20), new Vector2(24, 20), new Vector2(26, 20), new Vector2(28, 20), new Vector2(30, 20), new Vector2(32, 20), new Vector2(34, 20), new Vector2(36, 20),
                new Vector2(4, 22), new Vector2(6, 22), new Vector2(8, 22), new Vector2(10, 22), new Vector2(12, 22), new Vector2(14, 22), new Vector2(16, 22), new Vector2(18, 22), new Vector2(20, 22), new Vector2(22, 22), new Vector2(24, 22), new Vector2(26, 22), new Vector2(28, 22), new Vector2(30, 22), new Vector2(32, 22), new Vector2(34, 22), new Vector2(36, 22),
                new Vector2(4, 24), new Vector2(6, 24), new Vector2(8, 24), new Vector2(10, 24), new Vector2(12, 24), new Vector2(14, 24), new Vector2(16, 24), new Vector2(18, 24), new Vector2(20, 24), new Vector2(22, 24), new Vector2(24, 24), new Vector2(26, 24), new Vector2(28, 24), new Vector2(30, 24), new Vector2(32, 24), new Vector2(34, 24), new Vector2(36, 24),
                new Vector2(2, 26), new Vector2(4, 26), new Vector2(6, 26), new Vector2(8, 26), new Vector2(10, 26), new Vector2(12, 26), new Vector2(14, 26), new Vector2(16, 26), new Vector2(18, 26), new Vector2(20, 26), new Vector2(22, 26), new Vector2(24, 26), new Vector2(26, 26), new Vector2(28, 26), new Vector2(30, 26), new Vector2(32, 26), new Vector2(34, 26),
                new Vector2(0, 28), new Vector2(2, 28), new Vector2(4, 28), new Vector2(6, 28), new Vector2(8, 28), new Vector2(10, 28), new Vector2(12, 28), new Vector2(14, 28), new Vector2(16, 28), new Vector2(18, 28), new Vector2(20, 28), new Vector2(22, 28), new Vector2(24, 28), new Vector2(26, 28), new Vector2(28, 28), new Vector2(30, 28), new Vector2(32, 28), new Vector2(34, 28),
                new Vector2(0, 30), new Vector2(2, 30), new Vector2(4, 30), new Vector2(6, 30), new Vector2(8, 30), new Vector2(10, 30), new Vector2(12, 30), new Vector2(14, 30), new Vector2(16, 30), new Vector2(18, 30), new Vector2(20, 30), new Vector2(22, 30), new Vector2(24, 30), new Vector2(26, 30), new Vector2(28, 30), new Vector2(30, 30),
                new Vector2(0, 32), new Vector2(2, 32), new Vector2(4, 32), new Vector2(6, 32), new Vector2(8, 32), new Vector2(10, 32), new Vector2(12, 32), new Vector2(14, 32), new Vector2(16, 32), new Vector2(18, 32), new Vector2(20, 32), new Vector2(22, 32), new Vector2(24, 32), new Vector2(26, 32), new Vector2(28, 32), new Vector2(30, 32), new Vector2(32, 32),
                new Vector2(2, 34), new Vector2(4, 34), new Vector2(6, 34), new Vector2(8, 34), new Vector2(10, 34), new Vector2(12, 34), new Vector2(14, 34), new Vector2(16, 34), new Vector2(18, 34), new Vector2(20, 34), new Vector2(22, 34), new Vector2(24, 34), new Vector2(26, 34), new Vector2(28, 34), new Vector2(30, 34), new Vector2(32, 34),
                new Vector2(4, 36), new Vector2(6, 36), new Vector2(8, 36), new Vector2(10, 36), new Vector2(12, 36), new Vector2(14, 36), new Vector2(16, 36), new Vector2(18, 36), new Vector2(20, 36), new Vector2(22, 36), new Vector2(24, 36), new Vector2(26, 36), new Vector2(28, 36), new Vector2(30, 36), new Vector2(32, 36),
                new Vector2(6, 38), new Vector2(8, 38), new Vector2(10, 38), new Vector2(12, 38), new Vector2(14, 38), new Vector2(16, 38), new Vector2(18, 38), new Vector2(20, 38), new Vector2(22, 38), new Vector2(24, 38), new Vector2(26, 38), new Vector2(28, 38), new Vector2(30, 38)
            };

            // Create transformation matrix: translate to origin, rotate, then translate to world position
            var transform = Matrix.CreateTranslation(-Size40.Center.X, -Size40.Center.Y, 0) *
                           Matrix.CreateRotationZ(_animationRotation) *
                           Matrix.CreateTranslation(MapPosition.X, MapPosition.Y, 0);

            var rectangles = new Rectangle[collisionPoints.Length];
            for (int i = 0; i < collisionPoints.Length; i++)
            {
                var transformedPoint = Vector2.Transform(collisionPoints[i], transform);
                rectangles[i] = new Rectangle((int)transformedPoint.X, (int)transformedPoint.Y, 1, 1);
            }
            return rectangles;
        }
    }

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
