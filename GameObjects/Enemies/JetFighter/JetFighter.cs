using Hands.Core;
using Hands.Core.Animation;
using Hands.Core.Managers.Collision;
using Hands.Core.Managers.Explosion;
using Hands.Core.Managers.Smoke;
using Hands.Core.Sprites;

namespace Hands.GameObjects.Enemies.JetFighter;

internal class JetFighter : IUpdate, IDraw, IMapPosition, ISleep, ICollision
{
    private readonly JetFighterInfo _info;
    private readonly Vector2 _velocity;

    public JetFighter(JetFighterInfo info)
    {
        MapPosition = new Vector2(info.X, info.Y);
        _info = info;
        WakeDistance = _info.WakeDistance <= 0f ? Global.World.GlobalWakeDistance : _info.WakeDistance;

        // Set up vertical downward movement
        _velocity = new Vector2(0, _info.MovementSpeed);
    }

    #region IUpdate

    public void Update(GameTime gameTime)
    {
        if (State == JetFighterState.Destroyed) return;
        if (State == JetFighterState.Asleep) return;

        // Move downward when active
        if (State == JetFighterState.Active)
        {
            UpdateMovement(gameTime);
            UpdateSmokeTrail();
            
            // Check if fighter has flown off the bottom of the screen
            var cameraBottom = Global.World.Camera.Position.Y + Global.Graphics.RenderHeight;
            if (MapPosition.Y > cameraBottom + 100) // Add buffer beyond visible area
            {
                Manager.Unregister(this);
            }
        }
    }

    private void UpdateMovement(GameTime gameTime)
    {
        // Simple vertical movement - no collision detection needed for downward flight
        float speed = _velocity.Y * gameTime.ElapsedGameTime.Milliseconds;
        MapPosition = new Vector2(MapPosition.X, MapPosition.Y + speed);
    }

    private void UpdateSmokeTrail()
    {
        // Emit smoke every frame for continuous con-trail effect
        // Position smoke at the rear of the fighter
        var smokePosition = MapPosition + Size30.Center + new Vector2(0, -15); // Behind the fighter
        
        // Create 5 smoke particles every frame in a small area behind the fighter
        var smokeInfo = new SmokeAreaInfo(smokePosition, 3f, 5, 0f);
        Global.World.SmokeManager.Register(smokeInfo);
    }

    #endregion

    #region IDraw

    public void Draw(SpriteBatch spriteBatch)
    {
        // Only draw if fighter is active or asleep - destroyed fighters disappear completely
        if (State.In(JetFighterState.Asleep, JetFighterState.Active))
        {
            DrawActive(spriteBatch);
        }
    }

    private void DrawActive(SpriteBatch spriteBatch)
    {
        // Get sprite frames
        var jetFrame = Sprite.Frames[0].SourceRectangle;
        var shadowFrame = Sprite.Frames[1].SourceRectangle;
        var shadowOffset = new Vector2(12, 12);
        
        // Draw the shadow first
        spriteBatch.Draw(Sprite.Texture, MapPosition + shadowOffset, shadowFrame, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
        
        // Draw the jet fighter sprite (no rotation needed for straight down movement)
        spriteBatch.Draw(Sprite.Texture, MapPosition, jetFrame, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
    }

    internal JetFighterSprite Sprite => Manager.Sprite;

    #endregion

    #region ISleep

    public float WakeDistance { get; init; }
    public bool IsAsleep { get; private set; } = true;

    public void OnSisterAwake()
    {
        if (State == JetFighterState.Destroyed) return;
        State = JetFighterState.Active;
        IsAsleep = false;

        Global.World.CollisionManager.Register(this);
    }

    public void OnSleep()
    {
        // Jet fighters don't go back to sleep once awakened - they just fly away
    }

    #endregion

    #region ICollision

    public void OnCollide(ICollision other)
    {
        if (State == JetFighterState.Destroyed) return;

        // Jet fighter collision causes immediate destruction
        State = JetFighterState.Destroyed;

        Global.World.CollisionManager.UnRegister(this);
        Manager.Unregister(this); // Remove from JetFighterManager

        Explode();
        Smoke();
    }

    private void Explode()
    {
        var explosionInfo = new ExplosionInfo(Center, 30);
        Global.World.ExplosionManager.Register(explosionInfo);
    }

    private void Smoke()
    {
        var smokeRadius = 30f * 1.5f;
        var smokeParticleCount = (int)(20 * 1.5f);
        var smokeInfo = new SmokeAreaInfo(Center, smokeRadius, smokeParticleCount, 0.1f);
        Global.World.SmokeManager.Register(smokeInfo);
    }

    public Rectangle Clayton => new Rectangle(MapPosition.ToPoint(), Size30.Point);

    public Rectangle[] CollisionRectangles =>
    [
        new Rectangle((int)MapPosition.X, (int)MapPosition.Y, 30, 30)
    ];

    public CollisionType CollisionType => CollisionType.JetFighter;
    public bool IsHot => State == JetFighterState.Active;
    public bool ShouldRemoveOnCollision => State == JetFighterState.Destroyed;

    #endregion

    #region IMapPosition

    public Vector2 MapPosition { get; private set; }
    public Vector2 Center => MapPosition + Size30.Center;

    #endregion

    // Properties   
    public JetFighterState State { get; private set; } = JetFighterState.Asleep;
    public float MovementSpeed => _info.MovementSpeed;
    internal JetFighterManager Manager => Global.World.JetFighterManager;
}
