using Hands.Core;
using Hands.Core.Managers.Collision;
using Hands.Core.Sprites;
using Hands.Sprites;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hands.GameObjects.Projectiles;
internal class ProjectileManager : ILoadContent, IUpdate, IDraw
{
    private List<Projectile> _objectCache = [];
    private List<Projectile> _projectiles = [];
    private readonly object _projectilesLock = new();

    public ProjectileManager()
    {
        const int initialCapacity = 200; // Adjust based on expected number of projectiles
        _objectCache = new List<Projectile>(initialCapacity);
        _projectiles = new List<Projectile>(initialCapacity);
        for (int i = 0; i < initialCapacity; i++)
        {
            _objectCache.Add(new Projectile());
        }
    }

    public void LoadContent(ContentManager contentManager)
    {
        var projectileTexture = contentManager.Load<Texture2D>("Projectiles");
        var projectileFrames = SpriteHelper.CreateFramesFromTexture(projectileTexture, Size8.Point);

        Sprite = new Dictionary<ProjectileType, ProjectileSprite>
        {
            { ProjectileType.RedBall, new ProjectileSprite(Size8.Point, projectileTexture, projectileFrames[0]) },
            { ProjectileType.BlueBall, new ProjectileSprite(Size8.Point, projectileTexture, projectileFrames[1]) },
            { ProjectileType.GreyBall, new ProjectileSprite(Size8.Point, projectileTexture, projectileFrames[2]) },
            { ProjectileType.Laser, new ProjectileSprite(Size8.Point, projectileTexture, projectileFrames[0]) },
        };
    }

    public void Update(GameTime gameTime)
    {
        // Too many weird things happen with parallel updates, so we will use a simple foreach loop
        foreach (var projectile in _projectiles)
        {
            projectile?.Update(gameTime);
        }

        // Unregister all projectiles marked for deletion
        var toRemove = _projectiles.Where(p => p?.MarkForDeletion ?? false).ToList();
        foreach (var projectile in toRemove)
        {
            Unregister(projectile);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var projectile in _projectiles)
        {
            projectile?.Draw(spriteBatch);
        }
    }

    public void Register(ProjectileInfo projectileInfo)
    {
        // Take advantage of the object cache to reuse projectiles
        Projectile projectile;
        if (_objectCache.Any())
        {
            projectile = _objectCache[0];
            _objectCache.RemoveAt(0);
        }
        else
        {
            projectile = new Projectile();
        }
        projectile.Activate(projectileInfo);
        _projectiles.Add(projectile);
    }

    public void Unregister(Projectile projectile)
    {
        lock (_projectilesLock)
        {
            _objectCache.Add(projectile);
            if (_projectiles.Contains(projectile))
                _projectiles.Remove(projectile);
        }
    }

    public Dictionary<ProjectileType, ProjectileSprite> Sprite { get; private set; }
}

public record ProjectileSprite(Point Size, Texture2D Texture, SpriteFrame Frame);

internal record ProjectileInfo(ProjectileType ProjectileType, Vector2 MapPosition, Vector2 Vector, float Damage, CollisionType CollisionType);

internal enum ProjectileType
{
    Laser,
    RedBall,
    BlueBall,
    GreyBall
}