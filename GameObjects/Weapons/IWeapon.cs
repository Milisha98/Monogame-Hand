using Hands.Core;

namespace Hands.GameObjects.Weapons;
internal interface IWeapon : ILoadContent, IUpdate, IDraw
{
    public float ShootSpeed { get; }
    public float Damage { get; }
}
