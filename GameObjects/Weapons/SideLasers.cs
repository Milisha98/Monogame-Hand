using Hands.Core;
using Hands.Core.Animation;
using Hands.Core.Managers.Collision;
using Hands.Core.Sprites;
using Hands.GameObjects.Projectiles;
using Microsoft.Xna.Framework.Content;
using Input = Microsoft.Xna.Framework.Input;

namespace Hands.GameObjects.Weapons;
internal class SideLasers : IWeapon
{
    private static readonly WeaponInfo _defaultWeaponInfo = new(Damage: 1f, ShootDelay: 0.2f, ShootVelocity: 15f);
    private readonly WeaponInfo _weaponInfo;
    private readonly Tween _shootTween;

    public SideLasers() : this(_defaultWeaponInfo) { }
    public SideLasers(WeaponInfo weaponInfo)
    {
        _weaponInfo = weaponInfo;
        _shootTween = new Tween(TimeSpan.FromSeconds(weaponInfo.ShootDelay));
        _shootTween.OnCompleted += () => OnTweenCompleted();
    }
    public void LoadContent(ContentManager contentManager)
    {
        // Do nothing for this weapon
    }

    public void Update(GameTime gameTime)
    {
        _shootTween.Update(gameTime);
        bool isSpaceKeyPressed = KeyboardController.CheckKeyDown(Input.Keys.Space, Input.Keys.Enter);
        if (isSpaceKeyPressed && CanShoot)
        {
            CanShoot = false;
            _shootTween.Reset();

            // Create and shoot the projectile
            var left  = new ProjectileInfo(ProjectileType.Laser, MapPosition, new Vector2(0, -ShootVelocity), 1f, CollisionType.ProjectilePlayer);
            var right = new ProjectileInfo(ProjectileType.Laser, MapPosition + Size48.Width, new Vector2(0, -ShootVelocity), 1f, CollisionType.ProjectilePlayer);
            Global.World.ProjectileManager.Register(left);
            Global.World.ProjectileManager.Register(right);
        }
    }

    public void DrawShadow(SpriteBatch spriteBatch)
    {
        var shadowOffset = new Vector2(12, 12);
        spriteBatch.Draw(Global.World.Player.Texture, MapPosition + shadowOffset, Global.World.Player.Frames[3].SourceRectangle, Color.White);      // Left Shadow
    }
    public void DrawWeapon(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Global.World.Player.Texture, MapPosition, Global.World.Player.Frames[2].SourceRectangle, Color.White);                     // Right Laser
    }

    #region Events

    public void OnTweenCompleted()
    {
        CanShoot = true;
        _shootTween.Reset();
    }

    #endregion

    public bool     CanShoot        { get; private set; }
    public float    ShootVelocity   => _weaponInfo.ShootVelocity;
    public float    ShootDelay      => _weaponInfo.ShootDelay;
    public float    Damage          => _weaponInfo.Damage;
    public Player   Player          => Global.World.Player;
    public Vector2 MapPosition      => Player.MapPosition - Size48.Size;
}
