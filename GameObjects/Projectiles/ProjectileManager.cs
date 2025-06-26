using Hands.Core;
using Hands.Core.Managers.Collision;
using Hands.Core.Sprites;
using Hands.Sprites;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hands.GameObjects.Projectiles;
internal class ProjectileManager : ILoadContent, IUpdate, IDraw
{
    private List<Projectile> _projectiles = [];

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
        Parallel.ForEach(_projectiles, projectile =>
        {
            projectile.Update(gameTime);
        });
    }
    public void Draw(SpriteBatch spriteBatch)
    {
        Parallel.ForEach(_projectiles, projectile =>
        {
            projectile.Draw(spriteBatch);
        });
    }

    public void Register(ProjectileInfo projectileInfo)
    {
        int count = projectileInfo.ProjectileType == ProjectileType.Laser ? 8 : 1;
        for (int i = 0; i < count; i++)
        {
            var newInfo = projectileInfo with { MapPosition = projectileInfo.MapPosition + new Vector2(0, i * 4) };     // Offset for lasers
            var projectile = new Projectile(newInfo);
            _projectiles.Add(projectile);
            Global.World.CollisionManager.Register(projectile);
        }

    }

    public void Unregister(Projectile projectile)
    {
        _projectiles.Remove(projectile);
        Global.World.CollisionManager.UnRegister(projectile);
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