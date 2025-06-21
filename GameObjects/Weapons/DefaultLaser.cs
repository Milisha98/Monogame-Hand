using Hands.Core;
using Hands.Core.Animation;
using Hands.Core.Sprites;

using Input = Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace Hands.GameObjects.Weapons;
internal class DefaultLaser : IWeapon
{
    private static readonly WeaponInfo _defaultWeaponInfo = new(Damage: 1f, ShootSpeed: 0.5f);
    private readonly WeaponInfo _weaponInfo;
    private readonly Tween _shootTween;

    public DefaultLaser() : this(_defaultWeaponInfo) { }
    public DefaultLaser(WeaponInfo weaponInfo)
    {
        _weaponInfo = weaponInfo;
        _shootTween = new Tween(TimeSpan.FromSeconds(ShootSpeed));
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
            // TODO: Add Projectile
            System.Diagnostics.Debug.WriteLine("DefaultLaser: Shoot action triggered.");
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Do nothing for this default weapon
    }

    #region Events

    public void OnTweenCompleted()
    {
        CanShoot = true;
        _shootTween.Reset();
    }

    #endregion
    
    public bool     CanShoot    { get; private set; }
    public float    ShootSpeed  => _weaponInfo.ShootSpeed;
    public float    Damage      => _weaponInfo.Damage;
    public Player   Player      => Global.World.Player;
    public Vector2  MapPosition => Player.MapPosition + Size48.HalfWidth;
}
