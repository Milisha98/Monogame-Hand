using Hands.Sprites;
using System.Collections.Generic;
using System.Linq;

namespace Hands.Core.Managers.Collision;
public class CollisionManager : IDraw
{
    public List<ICollision> _collisions = new();
    public void RegisterCollision(ICollision collision)
    {
        _collisions.Add(collision);
    }
    public void RemoveCollision(ICollision collision)
    {
        _collisions.Remove(collision);
    }

    public CollisionType IsClaytonCollision(Rectangle rectangle)
    {
        var overlapping = _collisions.Where(c => c.Clayton.Intersects(rectangle));
        foreach (var clayton in overlapping)
        {
            return clayton.CollisionType;
        }
        return CollisionType.None;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (Global.DebugShowCollisionBoxes == false) return;
        Texture2D texture = spriteBatch.BlankTexture();
        foreach (Rectangle clayton in _collisions.Select(c => c.Clayton))
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
    Projectile
}