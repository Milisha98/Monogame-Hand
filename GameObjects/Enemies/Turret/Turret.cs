using Hands.Core;
using Hands.Core.Animation;
using Hands.Core.Sprites;
using Hands.Sprites;
using System;
using System.Diagnostics;

namespace Hands.GameObjects.Enemies.Turret;
internal class Turret : IUpdate, IDraw
{
    private readonly TurretInfo _info;
    private readonly Workflow<TurretState> _wf;

    private Vector2 _animationDoorOffset = Vector2.Zero;
    private Color _animationTurretColor = Color.Black;
    private float _animationCannonRotation = 0f;

    // Temporary
    private readonly Tween _wake;

    internal Turret(TurretInfo info)
    {
        MapPosition = new Vector2(info.X, info.Y);

        _wf = new(WorkflowStages)
        {
            IsActive = false
        };
        _wf.OnStateChanged += state => State = state;

        // Wakes up after 1 second
        _wake = new Tween(TimeSpan.FromSeconds(1));
        _wake.OnCompleted += () => OnSisterAwake();
        _info = info;
    }

    #region IUpdate

    public void Update(GameTime gameTime)
    {
        _wake.Update(gameTime);
        _wf.Update(gameTime);

        if (State == TurretState.Closed) return;

        UpdateOpening();
        UpdateRising();

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

        _animationDoorOffset = Size64.HalfWidth * _wf.CurrentPercent;

    }
    private void UpdateRising()
    {
        if (State != TurretState.Raising) return;

        int value = (int)(255f * _wf.CurrentPercent);
        _animationTurretColor = new Color(value, value, value);
    }

    #endregion

    #region IDraw
    public void Draw(SpriteBatch spriteBatch)
    {

        // Draw Mount
        spriteBatch.Draw(Sprite.Texture, MapPosition, Sprite.Frames[0].SourceRectangle, Color.White);

        // If turret is not destroyed, draw the body and cannon
        if (State != TurretState.Destroyed)
        { 
            // Draw Turret Body
            spriteBatch.Draw(Sprite.Texture, MapPosition, Sprite.Frames[1].SourceRectangle, _animationTurretColor);

            // Draw Turret Cannon
            spriteBatch.Draw(Sprite.Texture, MapPosition + Size64.Center, Sprite.Frames[2].SourceRectangle, _animationTurretColor, _animationCannonRotation, Size64.Center, 1f, SpriteEffects.None, 0);
        }
        else
        {
            // TODO: Draw Destroyed Turret
        }

        // Draw Doors
        Vector2 leftDoorPosition = MapPosition - Size64.HalfWidth - _animationDoorOffset;
        Vector2 rightDoorPosition   = MapPosition + Size64.HalfWidth + _animationDoorOffset;
        spriteBatch.Draw(Sprite.Texture, leftDoorPosition,  Sprite.Frames[3].SourceRectangle, Color.White);
        spriteBatch.Draw(Sprite.Texture, rightDoorPosition, Sprite.Frames[4].SourceRectangle, Color.White);
    }

    #endregion

    #region Workflow Stages

    private static WorkflowStage<TurretState>[] WorkflowStages
    {
        get =>
            [ new (TurretState.Opening, TimeSpan.FromMilliseconds(500)),
              new (TurretState.Raising, TimeSpan.FromMilliseconds(500))
            ];
    }

    #endregion

    #region Events

    private void OnSisterAwake()
    {
        _wf.IsActive = true;
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

}


