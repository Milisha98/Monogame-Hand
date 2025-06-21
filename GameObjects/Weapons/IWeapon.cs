using Hands.Core;

namespace Hands.GameObjects.Weapons;
internal interface IWeapon : ILoadContent, IUpdate, IDraw
{
    public float ShootVelocity { get; }
    public float ShootDelay { get; }
    public float Damage { get; }
}
