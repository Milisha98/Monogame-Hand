using Hands.Core.Sprites;
using Hands.Core.Managers.Collision;
using Hands.GameObjects.Weapons;

namespace Hands.Core.Managers.WeaponSpawn;

public class WeaponSpawn : IUpdate, IDraw, IMapPosition, ICollision
{
    private string _weaponType = string.Empty;

    public void Activate(Vector2 position, string weaponType)
    {
        MapPosition = position;
        _weaponType = weaponType;
        IsComplete = false;
    }

    public void Update(GameTime gameTime)
    {
        if (IsComplete) return;

        // Weapon spawns don't update over time - they persist until picked up
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsComplete) return;

        // Get the correct frame for this weapon type
        int frameIndex = WeaponSpawnManager.GetFrameForWeaponType(_weaponType);
        spriteBatch.Draw(Sprite.Texture, MapPosition, Sprite.Frames[frameIndex].SourceRectangle, Color.White, 0, Size32.Center, 1f, SpriteEffects.None, 0);
    }

    public bool IsComplete { get; private set; }
    public Vector2 MapPosition { get; private set; }
    public Vector2 Center => MapPosition + Size32.Center;
    public string WeaponType => _weaponType;
    internal WeaponSpawnSprite Sprite => Global.World.WeaponSpawnManager.Sprite;

    public void MarkForRemoval()
    {
        IsComplete = true;
    }

    #region ICollision

    public Rectangle Clayton => new Rectangle((MapPosition - Size32.Center).ToPoint(), Size32.Point);

    public Rectangle[] CollisionRectangles =>
        new Rectangle[]
        {
            new Rectangle((int)(MapPosition.X - Size32.Center.X), (int)(MapPosition.Y - Size32.Center.Y), Size32.Point.X, Size32.Point.Y)
        };

    public CollisionType CollisionType => CollisionType.WeaponSpawn;

    public bool IsHot => !IsComplete;

    public bool ShouldRemoveOnCollision => IsComplete;

    public void OnCollide(ICollision other)
    {
        // Only react to Player collisions
        if (other.CollisionType == CollisionType.Player)
        {
            // Give weapon to player based on weapon type
            GiveWeaponToPlayer(WeaponType);
            
            System.Diagnostics.Debug.WriteLine($"Player picked up weapon: {WeaponType}");
            
            // Mark for removal - the manager will handle unregistering from collision system
            MarkForRemoval();
        }
    }

    private void GiveWeaponToPlayer(string weaponType)
    {
        var player = Global.World.Player;
        
        switch (weaponType)
        {
            case "W":
                player.MainWeapon = new SideLasers();
                player.MainWeapon.LoadContent(null); // LoadContent doesn't need ContentManager for SideLasers
                break;
            
            // Add more weapon types here in the future:
            // case "S":
            //     player.MainWeapon = new SomeOtherWeapon();
            //     break;
            
            default:
                System.Diagnostics.Debug.WriteLine($"Unknown weapon type: {weaponType}");
                break;
        }
    }

    #endregion
}
