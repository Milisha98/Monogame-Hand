using Hands.Sprites;
using System.Collections.Generic;
using System.Linq;

namespace Hands.Core.Managers.Collision;
public class CollisionManager : IDraw
{
    private Dictionary<Rectangle, CollisionType> _claytons = new();
    public void RegisterCollision(Rectangle rectangle, CollisionType collisionType)
    {
        _claytons[rectangle] = collisionType;
    }
    public void RemoveCollision(Rectangle rectangle)
    {
        _claytons.Remove(rectangle);
    }

    public CollisionType IsClaytonCollision(Rectangle rectangle)
    {
        var overlapping = _claytons.Keys.ToList().Where(c => c.Intersects(rectangle));
        foreach (var clayton in overlapping)
        {
            return _claytons[clayton];
        }
        return CollisionType.None;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (Global.DebugShowCollisionBoxes == false) return;
        Texture2D texture = spriteBatch.BlankTexture();
        foreach (Rectangle clayton in _claytons.Keys)
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