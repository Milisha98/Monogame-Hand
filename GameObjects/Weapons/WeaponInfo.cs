namespace Hands.GameObjects.Weapons;
internal record WeaponInfo
    (float Damage, 
     float ShootSpeed, 
     int MaxAmmo = int.MaxValue);
