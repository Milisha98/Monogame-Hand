using Hands.Core;
using Hands.Core.Managers.Collision;
using Hands.Core.Managers.Explosion;
using Hands.Core.Managers.Smoke;
using Hands.Core.Sprites;
using Hands.Sprites;
using Microsoft.Xna.Framework.Graphics;

namespace Hands.GameObjects.Enemies.SideGun;

internal class SideGun : IUpdate, IDraw, IMapPosition, ISleep, ICollision
{
    private readonly SideGunInfo _info;

    public SideGun(SideGunInfo info)
    {
        MapPosition = new Vector2(info.X, info.Y);
        Orientation = info.Orientation;
        _info = info;
        WakeDistance = _info.WakeDistance <= 0f ? Global.World.GlobalWakeDistance : _info.WakeDistance;
    }

    #region IUpdate

    public void Update(GameTime gameTime)
    {
        if (State == SideGunState.Destroyed) return;
        // No complex logic needed for now
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
        
        // Draw idle sprite (frame 0)
        spriteBatch.Draw(
            Sprite.Texture,
            MapPosition,
            Sprite.Frames[0].SourceRectangle,
            Color.White,
            0f,
            Vector2.Zero,
            1f,
            spriteEffects,
            0f
        );

        DrawCollisionBox(spriteBatch);
    }

    private void DrawActive(SpriteBatch spriteBatch)
    {
        var spriteEffects = Orientation == SideGunOrientation.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        
        // Draw active sprite (frame 1 if available, otherwise frame 0)
        var frameIndex = Sprite.Frames.ContainsKey(1) ? 1 : 0;
        
        spriteBatch.Draw(
            Sprite.Texture,
            MapPosition,
            Sprite.Frames[frameIndex].SourceRectangle,
            Color.White,
            0f,
            Vector2.Zero,
            1f,
            spriteEffects,
            0f
        );

        DrawCollisionBox(spriteBatch);
    }

    private void DrawDestroyed(SpriteBatch spriteBatch)
    {
        var spriteEffects = Orientation == SideGunOrientation.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        
        // Draw destroyed sprite (frame 2 if available, otherwise frame 0 with darker color)
        var frameIndex = Sprite.Frames.ContainsKey(2) ? 2 : 0;
        var destroyedColor = Sprite.Frames.ContainsKey(2) ? Color.White : Color.Gray;
        
        spriteBatch.Draw(
            Sprite.Texture,
            MapPosition,
            Sprite.Frames[frameIndex].SourceRectangle,
            destroyedColor,
            0f,
            Vector2.Zero,
            1f,
            spriteEffects,
            0f
        );

        DrawCollisionBox(spriteBatch);
    }

    private void DrawCollisionBox(SpriteBatch spriteBatch)
    {
        if (Global.DebugShowCollisionBoxes)
        {
            Texture2D texture = spriteBatch.BlankTexture();
            foreach (var r in CollisionRectangles)
            {
                spriteBatch.Draw(texture, r, Color.Yellow);
            }
        }
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
        Global.World.CollisionManager.Register(this);
    }

    public void OnSleep()
    {
        if (State == SideGunState.Destroyed) return;
        State = SideGunState.Idle;
        IsAsleep = true;
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

    // Properties
    public string ID => _info.ID;
    public SideGunState State { get; private set; } = SideGunState.Idle;
    public SideGunOrientation Orientation { get; private set; }
    internal SideGunManager Manager => Global.World.SideGunManager;
    internal SideGunSprite Sprite => Manager.Sprite;
}
