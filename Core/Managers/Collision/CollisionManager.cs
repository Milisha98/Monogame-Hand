using Hands.Sprites;
using System.Collections.Generic;
using System.Linq;

namespace Hands.Core.Managers.Collision;
public class CollisionManager : IUpdate, IDraw
{
    public List<ICollision> _cold = new();
    public List<ICollision> _hot = new();
    public void Register(ICollision collision)
    {
        CollisionType[] hot =
            [
                CollisionType.ProjectilePlayer,
                CollisionType.ProjectileEnemy,
                CollisionType.Player
            ];

        if (hot.Contains(collision.CollisionType))
            _hot.Add(collision);
        else
             _cold.Add(collision);
    }
    public void UnRegister(ICollision collision)
    {
        _hot.Remove(collision);
        _cold.Remove(collision);
    }

    public ICollision? CheckClaytonsCollision(ICollision a)
    {
        var overlapping = _cold.Where(c => c.Clayton.Intersects(a.Clayton));
        foreach (var clayton in overlapping)
        {
            return clayton;
        }
        return null;
    }

    public bool IsCollisionWeCareAbout(CollisionType a, CollisionType b)
    {
        if (a == b) return false;                       // No self-collision
        if (a == CollisionType.None || b == CollisionType.None) return false;

        return (a, b) switch
        {
            (CollisionType.ProjectilePlayer,    CollisionType.Wall)             => true,
            (CollisionType.ProjectilePlayer,    CollisionType.Mount)            => false,
            (CollisionType.ProjectilePlayer,    CollisionType.Turret)           => true,
            (CollisionType.ProjectilePlayer,    CollisionType.ProjectileEnemy)  => true,
            (CollisionType.ProjectilePlayer,    CollisionType.Player)           => false,
            (CollisionType.ProjectileEnemy,     CollisionType.Wall)             => true,
            (CollisionType.ProjectileEnemy,     CollisionType.Mount)            => false,
            (CollisionType.ProjectileEnemy,     CollisionType.Turret)           => false,
            (CollisionType.ProjectileEnemy,     CollisionType.ProjectilePlayer) => true,
            (CollisionType.ProjectileEnemy,     CollisionType.Player)           => true,
            (CollisionType.Player,              CollisionType.Wall)             => true,
            (CollisionType.Player,              CollisionType.Mount)            => true,
            (CollisionType.Player,              CollisionType.Turret)           => true,
            (CollisionType.Player,              CollisionType.ProjectilePlayer) => false,
            (CollisionType.Player,              CollisionType.ProjectileEnemy)  => true,
            (_, _) => false
        };
    }

    public void Update(GameTime gameTime)
    {
        // For each hot, I want to see if it intersects with a cold (Claytons)
        foreach (var hot in _hot)
        {
            var collision = CheckClaytonsCollision(hot);
            if (collision is null) continue;
            bool isClaytonCollision = IsCollisionWeCareAbout(hot.CollisionType, collision.CollisionType);
            if (isClaytonCollision)
            {
                // Handle the collision, e.g., log it, apply effects, etc.
                // For now, we just print it to the console
                System.Diagnostics.Debug.WriteLine($"Collision detected: {hot.CollisionType} with {collision.CollisionType} at {hot.Clayton}");
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (Global.DebugShowCollisionBoxes == false) return;
        Texture2D texture = spriteBatch.BlankTexture();
        foreach (Rectangle clayton in _hot.Select(c => c.Clayton))
        {
            spriteBatch.Draw(texture, clayton, Color.Red);
        }
    }


}

public enum CollisionType
{
    None,
    Wall,
    Mount,
    Turret,
    ProjectilePlayer,
    ProjectileEnemy,
    Player
}