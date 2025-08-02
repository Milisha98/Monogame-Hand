using Hands.Sprites;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hands.Core.Managers.WeaponSpawn;
public class WeaponSpawnManager : ILoadContent, IUpdate, IDraw
{
    private List<WeaponSpawn> _objectCache = [];
    private List<WeaponSpawn> _weaponSpawns = [];

    public WeaponSpawnManager()
    {
        const int initialCapacity = 2;        // Initial capacity for the weapon spawns
        _weaponSpawns = new List<WeaponSpawn>(initialCapacity);
        _objectCache = new List<WeaponSpawn>(initialCapacity);
        for (int i = 0; i < initialCapacity; i++)
        {
            _objectCache.Add(new WeaponSpawn());
        }
    }

    public void Register(WeaponSpawnInfo info)
    {
        WeaponSpawn weaponSpawn;
        if (_objectCache.Any())
        {
            // Recycle an existing weapon spawn
            weaponSpawn = _objectCache[0];
            _objectCache.RemoveAt(0);
        }
        else
        {
            // Create a new weapon spawn if the cache is empty
            weaponSpawn = new WeaponSpawn();
        }
        weaponSpawn.Activate(info.Position, info.WeaponType);
        _weaponSpawns.Add(weaponSpawn);
        
        // Register with collision manager for pickup detection
        Global.World.CollisionManager.Register(weaponSpawn);
    }

    public void LoadContent(ContentManager contentManager)
    {
        Sprite.LoadContent(contentManager);
    }

    public void Update(GameTime gameTime)
    {
        // Single loop to update and handle completed spawns
        for (int i = _weaponSpawns.Count - 1; i >= 0; i--)
        {
            var weaponSpawn = _weaponSpawns[i];
            weaponSpawn.Update(gameTime);
            
            if (weaponSpawn.IsComplete)
            {
                // Unregister from collision system
                Global.World.CollisionManager.UnRegister(weaponSpawn);
                
                // Move to cache
                _objectCache.Add(weaponSpawn);
                
                // Remove from active list (iterating backwards so safe to remove)
                _weaponSpawns.RemoveAt(i);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Parallel.ForEach(_weaponSpawns, weaponSpawn =>
        {
            weaponSpawn.Draw(spriteBatch);
        });
    }

    /// <summary>
    /// Maps weapon type letters to frame indices in the GameIcons texture
    /// </summary>
    public static int GetFrameForWeaponType(string weaponType)
    {
        return weaponType switch
        {
            "W" => 0,  // Frame 0 is the letter "W"
            // Add more weapon types here in the future:
            _ => 0      // Default to frame 0 if unknown type
        };
    }

    //
    // Properties
    //
    public WeaponSpawnSprite Sprite { get; init; } = new();
}

public record WeaponSpawnInfo(Vector2 Position, string WeaponType);

public class WeaponSpawnSprite : ILoadContent
{
    public void LoadContent(ContentManager contentManager)
    {
        Texture = contentManager.Load<Texture2D>("GameIcons");
        Frames = SpriteHelper.CreateFramesFromTexture(Texture, new Point(32, 32));
    }

    public Texture2D Texture { get; set; }
    public Dictionary<int, SpriteFrame> Frames { get; private set; }
}
