using Hands.Core;
using Hands.Core.Animation;
using Hands.Core.Managers.Collision;
using Hands.Core.Sprites;
using Hands.GameObjects.Projectiles;
using Microsoft.Xna.Framework.Content;
using Input = Microsoft.Xna.Framework.Input;

namespace Hands.GameObjects.Weapons;
internal class DefaultWeapon : IWeapon
{
    private static readonly WeaponInfo _defaultWeaponInfo = new(Damage: 1f, ShootDelay: 0.25f, ShootVelocity: 5f);
    private readonly WeaponInfo _weaponInfo;
    private readonly Tween _shootTween;

    public DefaultWeapon() : this(_defaultWeaponInfo) { }
    public DefaultWeapon(WeaponInfo weaponInfo)
    {
        _weaponInfo = weaponInfo;
        _shootTween = new Tween(TimeSpan.FromSeconds(weaponInfo.ShootDelay));
        _shootTween.OnCompleted += () => OnTweenCompleted();
    }
    public void LoadContent(ContentManager contentManager)
    {
        // Do nothing for this default weapon
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
            var info = new ProjectileInfo(ProjectileType.RedBall, MapPosition, new Vector2(0, -ShootVelocity), 1f, CollisionType.ProjectilePlayer);
            Global.World.ProjectileManager.Register(info);
        }
    }

    public void DrawShadow(SpriteBatch spriteBatch)
    {
        // Do Nothing
    }
    public void DrawWeapon(SpriteBatch spriteBatch)
    {
        // Do Nothing
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
    public Vector2  MapPosition     => Player.MapPosition - Size48.Size + Size48.HalfWidth - Size8.HalfWidth;
}
