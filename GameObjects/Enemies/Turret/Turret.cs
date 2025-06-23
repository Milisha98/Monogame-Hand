using Hands.Core;
using Hands.Core.Animation;
using Hands.Core.Managers.Collision;
using Hands.Core.Managers.Explosion;
using Hands.Core.Managers.Smoke;
using Hands.Core.Sprites;
using Hands.GameObjects.Projectiles;

namespace Hands.GameObjects.Enemies.Turret;

internal class Turret : IUpdate, IDraw, IMapPosition, ISleep, ICollision
{
    private readonly TurretInfo _info;
    private readonly Workflow<TurretState> _openWorkflow;
    private readonly Workflow<TurretState> _closeWorkflow;
    private readonly Tween _fireDelay;

    private Vector2 _animationDoorOffsetX = Vector2.Zero;
    private Vector2 _animationDoorOffsetY = Vector2.Zero;
    private Color _animationTurretColor = Color.Black;
    private float _animationCannonRotation = 0f;
    private float _damageRotation = Random.Shared.NextSingle() * MathF.Tau;

    public Turret(TurretInfo info)
    {
        MapPosition = new Vector2(info.X, info.Y);
        Style = info.Style;

        // Initialize the workflow stages for the turret
        _openWorkflow = new(OpenWorkflowStages)
        {
            IsActive = false
        };
        _openWorkflow.OnStateChanged += state => OnChangeState(state);
        _openWorkflow.OnCompleted += () => OnChangeState(TurretState.Active);

        // Initialize the close workflow (if needed)
        _closeWorkflow = new(CloseWorkflowStages)
        {
            IsActive = false
        };
        _closeWorkflow.OnStateChanged += state => OnChangeState(state);
        _closeWorkflow.OnCompleted += () => OnChangeState(TurretState.Closed);

        _info = info;
        WakeDistance = _info.WakeDistance <= 0f ? Global.World.GlobalWakeDistance : _info.WakeDistance;

        _fireDelay = new Tween(TimeSpan.FromSeconds(RateOfFire));
    }

    #region IUpdate

    public void Update(GameTime gameTime)
    {
        if (State == TurretState.Destroyed) return;

        _openWorkflow.Update(gameTime);
        _closeWorkflow.Update(gameTime);
        _fireDelay.Update(gameTime);

        if (State == TurretState.Closed) return;

        UpdateOpening();
        UpdateRising();
        UpdateLowering();
        UpdateClosing();
        UpdateActive();

    }

    private void UpdateActive()
    {
        if (State != TurretState.Active) return;

        Vector2 playerPosition = Global.World.Player.MapPosition;
        Vector2 direction = playerPosition - MapPosition;

        _animationCannonRotation = MathF.Atan2(direction.Y, direction.X);

        if (_fireDelay.IsComplete)
        {
            Shoot(direction);
            _fireDelay.Reset();
        }

    }

    private void UpdateOpening()
    {
        if (State != TurretState.Opening) return;

        _animationDoorOffsetX = Size32.Width * _openWorkflow.CurrentPercent;
        _animationDoorOffsetY = Size32.Height * _openWorkflow.CurrentPercent;
        //if (ID == "2") Debug.WriteLine($"Turret {ID} Opening: OffsetX={_animationDoorOffsetX}, OffsetY={_animationDoorOffsetY}");
    }
    private void UpdateRising()
    {
        if (State != TurretState.Raising) return;

        int value = (int)(255f * _openWorkflow.CurrentPercent);
        _animationTurretColor = new Color(value, value, value);
        //if (ID == "2") Debug.WriteLine($"Turret {ID} Rising: Color={_animationTurretColor}");
    }

    private void UpdateLowering()
    {
        if (State != TurretState.Lowering) return;

        int value = (int)(255f - (255f * _closeWorkflow.CurrentPercent));
        _animationTurretColor = new Color(value, value, value);
        //if (ID == "2") System.Diagnostics.Debug.WriteLine($"Turret {ID} Lowering: Color={_animationTurretColor}");
    }

    private void UpdateClosing()
    {
        if (State != TurretState.Closing) return;
        _animationDoorOffsetX = Size32.Width * (1f - _closeWorkflow.CurrentPercent);
        _animationDoorOffsetY = Size32.Height * (1f - _closeWorkflow.CurrentPercent);
        //if (ID == "2") Debug.WriteLine($"Turret {ID} Closing: OffsetX={_animationDoorOffsetX}, OffsetY={_animationDoorOffsetY}");
    }

    private void Shoot(Vector2 direction)
    {
        var firePosition = MapPosition + Size64.Center + new Vector2(MathF.Cos(_animationCannonRotation), MathF.Sin(_animationCannonRotation)) * 32f;
        var fireVector = direction;
        if (fireVector != Vector2.Zero) fireVector.Normalize();
        fireVector *= ShootVelocity;
        var projectile = new ProjectileInfo(ProjectileType.BlueBall, firePosition, fireVector, 1f, CollisionType.ProjectileEnemy);
        Global.World.ProjectileManager.Register(projectile);
    }

    #endregion

    #region IDraw
    public void Draw(SpriteBatch spriteBatch)
    {
        switch (Style)
        {
            case TurretStyle.Style1:
                DrawStyle1(spriteBatch);
                break;
            // Add more styles here as needed
            default:
                DrawStyle2(spriteBatch);
                break;
        }
    }

    private void DrawStyle1(SpriteBatch spriteBatch)
    {

        // Draw Mount
        spriteBatch.Draw(Sprite.Texture, MapPosition, Sprite.Frames[0].SourceRectangle, Color.White);

        // Draw Turret
        if (State == TurretState.Destroyed)
            DrawDestroyed(spriteBatch);
        else
            DrawTurret(spriteBatch);

        // Draw Doors
        Vector2 leftDoorPosition = MapPosition - Size64.HalfWidth - _animationDoorOffsetX;
        Vector2 rightDoorPosition = MapPosition + Size64.HalfWidth + _animationDoorOffsetX;
        spriteBatch.Draw(Sprite.Texture, leftDoorPosition, Sprite.Frames[2].SourceRectangle, Color.White);
        spriteBatch.Draw(Sprite.Texture, rightDoorPosition, Sprite.Frames[3].SourceRectangle, Color.White);
    }

    private void DrawStyle2(SpriteBatch spriteBatch)
    {
        DrawStyle2Doors(spriteBatch);

        // Draw Mount
        spriteBatch.Draw(Sprite.Texture, MapPosition, Sprite.Frames[4].SourceRectangle, Color.White);

        DrawTurret(spriteBatch);
        DrawDestroyed(spriteBatch);
    }
    private void DrawStyle2Doors(SpriteBatch spriteBatch)
    {
        if (State.In(TurretState.Active, TurretState.Raising, TurretState.Lowering, TurretState.Destroyed))
            return;

        // Get the 32x32 Source Rectangles for the Doors
        var doorLeft = Sprite.Frames[6].SourceRectangle;
        var doorRight = Sprite.Frames[7].SourceRectangle;
        var srcTL = new Rectangle(doorLeft.X + 32, doorLeft.Y, 32, 32);
        var srcBL = new Rectangle(doorLeft.X + 32, doorLeft.Y + 32, 32, 32);
        var srcTR = new Rectangle(doorRight.X, doorRight.Y, 32, 32);
        var srcBR = new Rectangle(doorRight.X, doorRight.Y + 32, 32, 32);

        // Drawing Door Positions
        Vector2 posTL = MapPosition;
        Vector2 posBL = MapPosition + Size32.Height + _animationDoorOffsetY;
        Vector2 posTR = MapPosition + Size32.Width + _animationDoorOffsetX;
        Vector2 posBR = MapPosition + Size32.Width + Size32.Height + _animationDoorOffsetX + _animationDoorOffsetY;

        // Work out Position Offsets
        var ax = (int)_animationDoorOffsetX.X;
        var ay = (int)_animationDoorOffsetY.Y;

        int gutter = 1; // Gutter for the door edges
        srcTL = new Rectangle(srcTL.X + ax, srcTL.Y + ay, 32 - ax - gutter, 32 - ay - gutter);
        srcBL = new Rectangle(srcBL.X + ax, srcBL.Y, 32 - ax - gutter, 32 - ay - gutter);
        srcTR = new Rectangle(srcTR.X, srcTR.Y + ay, 32 - ax - gutter, 32 - ay - gutter);
        srcBR = new Rectangle(srcBR.X, srcBR.Y, 32 - ax - gutter, 32 - ay - gutter);

        spriteBatch.Draw(Sprite.Texture, posTL, srcTL, Color.White);
        spriteBatch.Draw(Sprite.Texture, posBL, srcBL, Color.White);
        spriteBatch.Draw(Sprite.Texture, posTR, srcTR, Color.White);
        spriteBatch.Draw(Sprite.Texture, posBR, srcBR, Color.White);

    }

    private void DrawTurret(SpriteBatch spriteBatch)
    {
        if (State.In(TurretState.Raising, TurretState.Lowering, TurretState.Active) == false) return;

        // Draw Turret Body
        spriteBatch.Draw(Sprite.Texture, MapPosition, Sprite.Frames[1].SourceRectangle, _animationTurretColor);

        // Draw Turret Cannon
        spriteBatch.Draw(Sprite.Texture, MapPosition + Size64.Center, Sprite.Frames[5].SourceRectangle, _animationTurretColor, _animationCannonRotation, Size64.Center, 1f, SpriteEffects.None, 0);
    }

    private void DrawDestroyed(SpriteBatch spriteBatch)
    {
        if (State != TurretState.Destroyed) return;

        // Draw Destroyed Turret Body
        spriteBatch.Draw(Sprite.Texture, MapPosition, Sprite.Frames[8].SourceRectangle, _animationTurretColor);

        // Draw Damage   
        spriteBatch.Draw(Sprite.Texture, MapPosition + Size64.Center, Sprite.Frames[9].SourceRectangle, _animationTurretColor, _damageRotation, Size64.Center, 1f, SpriteEffects.None, 0);
    }

    #endregion

    #region Workflow Stages

    private static WorkflowStage<TurretState>[] OpenWorkflowStages
    {
        get =>
            [ new (TurretState.Opening, TimeSpan.FromMilliseconds(250)),
              new (TurretState.Raising, TimeSpan.FromMilliseconds(250))
            ];
    }

    private static WorkflowStage<TurretState>[] CloseWorkflowStages
    {
        get =>
            [ new (TurretState.Lowering, TimeSpan.FromMilliseconds(500)),
              new (TurretState.Closing, TimeSpan.FromMilliseconds(250))
            ];
    }

    private void OnChangeState(TurretState state)
    {
        if (State == state) return; // No change

        //Debug.WriteLine($"Turret {ID} State from {State} -> {state}");

        // Handle collision registration based on state change
        if (State == TurretState.Active)
        {
            Global.World.CollisionManager.UnRegister(this);
        }
        if (state == TurretState.Active)
        {
            _fireDelay.IsActive = true;
            Global.World.CollisionManager.Register(this);
        }

        State = state;

        if (state == TurretState.Destroyed)
        {
            _closeWorkflow.IsActive = false;
            _closeWorkflow.Reset();
            _openWorkflow.IsActive = false;
            _openWorkflow.Reset();
        }
    }

    #endregion

    #region ISleep

    public float WakeDistance { get; init; }
    public bool IsAsleep { get; private set; } = true;
    public void OnSisterAwake()
    {
        if (State != TurretState.Closed) return;
        _openWorkflow.IsActive = true;
        _closeWorkflow.IsActive = false;
        _closeWorkflow.Reset();
        IsAsleep = false;
    }

    public void OnSleep()
    {
        if (State.In(TurretState.Closed, TurretState.Destroyed)) return;
        _openWorkflow.IsActive = false;
        _closeWorkflow.IsActive = true;
        _openWorkflow.Reset();
        IsAsleep = true;
    }

    #endregion

    #region ICollision

    public Rectangle Clayton => new Rectangle(MapPosition.ToPoint(), Size64.Point);

    public Rectangle[] CollisionRectangles => [Clayton];

    public CollisionType CollisionType => CollisionType.Turret;

    public bool IsHot => false;

    public void OnCollide(ICollision other)
    {
        if (State != TurretState.Active) return;

        var explosionInfo = new ExplosionInfo(MapPosition, 64);

        Global.World.ExplosionManager.Register(explosionInfo);

        Global.World.CollisionManager.UnRegister(this);
        OnChangeState(TurretState.Destroyed);

        var rectangle = new Rectangle((MapPosition - Size64.Center).ToPoint(), new Point(128, 128));
        var smokeInfo = new SmokeAreaInfo(rectangle, 50, 0.1f);
        Global.World.SmokeManager.Register(smokeInfo);
    }

    #endregion

    // Properties   
    public string ID { get => _info.ID; }
    public float RateOfFire { get => _info.RoF; }
    public TurretState State { get; private set; } = TurretState.Closed;
    public Vector2 MapPosition { get; private set; }
    internal TurretManager Manager => Global.World.TurretManager;
    internal TurretSprite Sprite => Manager.Sprite;
    internal Vector2 Target { get; private set; } = Vector2.Zero;
    public TurretStyle Style { get; set; } = TurretStyle.Style1;        // Default style
    public float ShootVelocity => 5f;

}


