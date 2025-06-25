using Hands.Core;

namespace Hands.GameObjects.Weapons;
internal interface IWeapon : ILoadContent, IUpdate
{
    public float ShootVelocity { get; }
    public float ShootDelay { get; }
    public float Damage { get; }

    void DrawShadow(SpriteBatch spriteBatch);
    void DrawWeapon(SpriteBatch spriteBatch);
}
