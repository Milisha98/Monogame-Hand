using Hands.Core;
using Hands.Core.Animation;
using Hands.Core.Sprites;

namespace Hands.GameObjects.Enemies.Turret;

internal class Turret : IUpdate, IDraw, IMapPosition, ISleep
{
    private readonly TurretInfo _info;
    private readonly Workflow<TurretState> _openWorkflow;
    private readonly Workflow<TurretState> _closeWorkflow;

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
        _openWorkflow.OnStateChanged += state => State = state;
        _openWorkflow.OnCompleted += () => State = TurretState.Active;

        // Initialize the close workflow (if needed)
        _closeWorkflow = new(CloseWorkflowStages)
        {
            IsActive = false
        };
        _closeWorkflow.OnStateChanged += state => State = state;
        _closeWorkflow.OnCompleted += () => State = TurretState.Closed;

        _info = info;
        WakeDistance = _info.WakeDistance <= 0f ? Global.World.GlobalWakeDistance : _info.WakeDistance;

    }

    #region IUpdate

    public void Update(GameTime gameTime)
    {
        _openWorkflow.Update(gameTime);
        _closeWorkflow.Update(gameTime);

        if (State == TurretState.Closed) return;

        UpdateOpening();
        UpdateRising();
        UpdateLowering();
        UpdateClosing();

        // For the time being, set the target to the mouse position
        var mousePosition = MouseController.MousePosition / Global.Graphics.Scale;
        var position = MapPosition + Size64.Center;
        var screenPosition = Global.World.Camera.WorldToScreenPosition(position);
        Vector2 direction = mousePosition - screenPosition;

        _animationCannonRotation = MathF.Atan2(direction.Y, direction.X);

    }

    private void UpdateOpening()
    {
        if (State != TurretState.Opening) return;

        _animationDoorOffsetX = Size32.Width * _openWorkflow.CurrentPercent;
        _animationDoorOffsetY = Size32.Height * _openWorkflow.CurrentPercent;

    }
    private void UpdateRising()
    {
        if (State != TurretState.Raising) return;

        int value = (int)(255f * _openWorkflow.CurrentPercent);
        _animationTurretColor = new Color(value, value, value);
    }

    private void UpdateLowering()
    {
        if (State != TurretState.Lowering) return;

        int value = (int)(255f - (255f * _closeWorkflow.CurrentPercent));
        _animationTurretColor = new Color(value, value, value);
    }

    private void UpdateClosing()
    {
        if (State != TurretState.Closing) return;
        _animationDoorOffsetX = Size32.Width * (1f - _closeWorkflow.CurrentPercent);
        _animationDoorOffsetY = Size32.Height * (1f - _closeWorkflow.CurrentPercent);
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
        if (State != TurretState.Raising && State != TurretState.Active) return;

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
            [ new (TurretState.Opening, TimeSpan.FromMilliseconds(500)),
              new (TurretState.Raising, TimeSpan.FromMilliseconds(500))
            ];
    }

    private static WorkflowStage<TurretState>[] CloseWorkflowStages
    {
        get =>
            [ new (TurretState.Lowering, TimeSpan.FromMilliseconds(500)),
              new (TurretState.Closing, TimeSpan.FromMilliseconds(500))
            ];
    }

    #endregion

    #region ISleep

    public float WakeDistance { get; init; }
    public bool IsAsleep { get; private set; } = true;
    private void OnSisterAwake()
    {
        if (State != TurretState.Closed) return;
        _openWorkflow.IsActive = true;
        _closeWorkflow.IsActive = false;
        _closeWorkflow.Reset();
        IsAsleep = false;
    }

    private void OnSleep()
    {
        if (State.In(TurretState.Closed, TurretState.Destroyed)) return;
        _openWorkflow.IsActive = false;
        _closeWorkflow.IsActive = true;
        _openWorkflow.Reset();
        IsAsleep = true;
    }

    void ISleep.OnSleep()
    {
        OnSleep();
    }

    void ISleep.OnSisterAwake()
    {
        OnSisterAwake();
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

}


