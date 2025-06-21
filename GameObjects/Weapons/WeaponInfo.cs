namespace Hands.GameObjects.Weapons;
internal record WeaponInfo
    (float Damage, 
     float ShootDelay,
     float ShootVelocity,
     int MaxAmmo = int.MaxValue);
