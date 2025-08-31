using Hands.Sprites;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hands.Core.Managers.Collision;
public class CollisionManager : IUpdate, IDraw
{
    public HashSet<ICollision> _cold = new();
    public HashSet<ICollision> _hot = new();

    public void Register(ICollision collision)
    {
        if (collision.IsHot)
            _hot.Add(collision);
        else
            _cold.Add(collision);
    }

    public void UnRegister(ICollision collision)
    {
        if (collision.IsHot)
            _hot.Remove(collision);
        else
            _cold.Remove(collision);
    }
    
    public ICollision CheckCollision(ICollision a)
    { 
        ICollision b = CheckClaytonsCollision(a);
        if (b is null) 
            return null;

        if (!IsCollisionWeCareAbout(a.CollisionType, b.CollisionType))
            return null;

        // Optimize by checking smaller collection first to minimize iterations
        var (outerRects, innerRects) = a.CollisionRectangles.Length <= b.CollisionRectangles.Length 
            ? (a.CollisionRectangles, b.CollisionRectangles)
            : (b.CollisionRectangles, a.CollisionRectangles);

        foreach (var rectOuter in outerRects)
        {
            foreach (var rectInner in innerRects)
            {
                if (rectOuter.Intersects(rectInner))
                    return b;
            }
        }

        return null;
    }

    /// <summary>
    /// Check collision specifically for movement blocking (used by CanMoveTo and CheckMoveTo)
    /// </summary>
    public ICollision CheckMovementCollision(ICollision a)
    { 
        ICollision b = CheckClaytonsCollision(a);
        if (b is null) 
            return null;

        if (!IsMovementBlockingCollision(a.CollisionType, b.CollisionType))
            return null;

        // Optimize by checking smaller collection first to minimize iterations
        var (outerRects, innerRects) = a.CollisionRectangles.Length <= b.CollisionRectangles.Length 
            ? (a.CollisionRectangles, b.CollisionRectangles)
            : (b.CollisionRectangles, a.CollisionRectangles);

        foreach (var rectOuter in outerRects)
        {
            foreach (var rectInner in innerRects)
            {
                if (rectOuter.Intersects(rectInner))
                    return b;
            }
        }

        return null;
    }

    public ICollision CheckClaytonsCollision(ICollision a)
    {
        foreach (var c in _cold)
            if (c.Clayton.Intersects(a.Clayton))
                return c;
        foreach (var c in _hot)
            if (c.Clayton.Intersects(a.Clayton) && !ReferenceEquals(c, a))
                return c;
        return null;
    }

    /// <summary>
    /// Checks if an object can move to a center position, returning the CollisionType of any collision
    /// </summary>
    /// <param name="proposedCenter">The proposed new center position</param>
    /// <param name="collisionType">The collision type of the moving object</param>
    /// <param name="size">The size of the collision rectangle</param>
    /// <returns>The CollisionType of the collided object, or null if no collision</returns>
    public CollisionType? CheckMoveTo(Vector2 proposedCenter, CollisionType collisionType, Point size)
    {
        var halfSize = new Vector2(size.X / 2f, size.Y / 2f);
        var proposedTopLeft = proposedCenter - halfSize;
        var proposedClayton = new Rectangle(proposedTopLeft.ToPoint(), size);
        
        var proposedCollision = new StaticCollision(proposedClayton, [], collisionType);
        var result = CheckMovementCollision(proposedCollision);
        
        return result?.CollisionType; // Return the CollisionType of what we collided with, or null
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
            (CollisionType.ProjectilePlayer,    CollisionType.Mobile)           => true,
            (CollisionType.ProjectilePlayer,    CollisionType.JetFighter)       => true,
            (CollisionType.ProjectileEnemy,     CollisionType.Wall)             => true,
            (CollisionType.ProjectileEnemy,     CollisionType.Mount)            => false,
            (CollisionType.ProjectileEnemy,     CollisionType.Turret)           => false,
            (CollisionType.ProjectileEnemy,     CollisionType.Mobile)           => false,
            (CollisionType.ProjectileEnemy,     CollisionType.ProjectilePlayer) => true,
            (CollisionType.ProjectileEnemy,     CollisionType.Player)           => true,
            (CollisionType.Player,              CollisionType.Wall)             => true,
            (CollisionType.Player,              CollisionType.Mount)            => true,
            (CollisionType.Player,              CollisionType.Turret)           => true,
            (CollisionType.Player,              CollisionType.ProjectilePlayer) => false,
            (CollisionType.Player,              CollisionType.ProjectileEnemy)  => true,
            (CollisionType.Player,              CollisionType.Mobile)           => true,
            (CollisionType.Player,              CollisionType.JetFighter)       => true,
            (CollisionType.Player,              CollisionType.WeaponSpawn)      => true,
            (CollisionType.Mobile,              CollisionType.Wall)             => true,
            (CollisionType.Mobile,              CollisionType.Mount)            => true,
            (CollisionType.Mobile,              CollisionType.Player)           => false,
            (CollisionType.Mobile,              CollisionType.Turret)           => false,
            (CollisionType.Mobile,              CollisionType.ProjectilePlayer) => true,
            (CollisionType.Mobile,              CollisionType.ProjectileEnemy)  => false,
            (_, _) => false
        };
    }

    /// <summary>
    /// Determines if a collision should block movement (for CheckMoveTo methods)
    /// </summary>
    public bool IsMovementBlockingCollision(CollisionType a, CollisionType b)
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
            (CollisionType.ProjectilePlayer,    CollisionType.Mobile)           => true,
            (CollisionType.ProjectileEnemy,     CollisionType.Wall)             => true,
            (CollisionType.ProjectileEnemy,     CollisionType.Mount)            => false,
            (CollisionType.ProjectileEnemy,     CollisionType.Turret)           => false,
            (CollisionType.ProjectileEnemy,     CollisionType.Mobile)           => false,
            (CollisionType.ProjectileEnemy,     CollisionType.ProjectilePlayer) => true,
            (CollisionType.ProjectileEnemy,     CollisionType.Player)           => true,
            (CollisionType.Player,              CollisionType.Wall)             => true,
            (CollisionType.Player,              CollisionType.Mount)            => true,
            (CollisionType.Player,              CollisionType.Turret)           => true,
            (CollisionType.Player,              CollisionType.ProjectilePlayer) => false,
            (CollisionType.Player,              CollisionType.ProjectileEnemy)  => true,
            (CollisionType.Player,              CollisionType.Mobile)           => true,
            (CollisionType.Player,              CollisionType.WeaponSpawn)      => false, // Don't block movement for weapon pickups
            (CollisionType.Mobile,              CollisionType.Wall)             => true,
            (CollisionType.Mobile,              CollisionType.Mount)            => true,
            (CollisionType.Mobile,              CollisionType.Player)           => true, // Block movement between mobiles and player
            (CollisionType.Mobile,              CollisionType.Turret)           => false,
            (CollisionType.Mobile,              CollisionType.ProjectilePlayer) => true,
            (CollisionType.Mobile,              CollisionType.ProjectileEnemy)  => false,
            (_, _) => false
        };
    }

    public void Update(GameTime gameTime)
    {
        var collisionPairs = new List<(ICollision hot, ICollision collision)>();

        // Step 1: Detect all collisions (now sequential instead of parallel)
        var hotObjects = _hot.ToList(); // Create snapshot to avoid collection modification
        foreach (var hot in hotObjects)
        {
            var collision = CheckCollision(hot);
            if (collision != null)
            {
                collisionPairs.Add((hot, collision));
            }
        }

        // Step 2: Process collisions sequentially to avoid race conditions
        var markForDestroy = new List<ICollision>();
        foreach (var (hot, collision) in collisionPairs)
        {
            // Double-check that objects are still valid (haven't been removed by another collision)
            if (!_hot.Contains(hot)) continue;
            
            // Handle the collision
            System.Diagnostics.Debug.WriteLine($"Collision detected: {hot.CollisionType} with {collision.CollisionType} at {hot.Clayton}");
            hot.OnCollide(collision);
            collision.OnCollide(hot);

            // Only remove objects that should be removed on collision
            if (_hot.Contains(hot) && hot.ShouldRemoveOnCollision)
                markForDestroy.Add(hot);
            if (_hot.Contains(collision) && collision.ShouldRemoveOnCollision)
                markForDestroy.Add(collision);
        }

        // Step 3: Remove all marked collisions after processing
        foreach (var item in markForDestroy.Distinct())
        {
            _hot.Remove(item);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (Global.DebugShowClaytonCollisionBoxes == false) return;
        Texture2D texture = spriteBatch.BlankTexture();
        
        // Draw hot entities in red
        foreach (Microsoft.Xna.Framework.Rectangle clayton in _hot.Select(c => c.Clayton))
        {
            spriteBatch.Draw(texture, clayton, Microsoft.Xna.Framework.Color.Red);
        }
        
        // Draw cold entities in blue
        foreach (Microsoft.Xna.Framework.Rectangle clayton in _cold.Select(c => c.Clayton))
        {
            spriteBatch.Draw(texture, clayton, Microsoft.Xna.Framework.Color.Blue);
        }
        
        // Draw detailed collision rectangles if enabled
        if (Global.DebugShowCollisionBoxes)
        {
            // Draw detailed collision rectangles for hot entities in yellow
            foreach (var entity in _hot)
            {
                foreach (var rect in entity.CollisionRectangles)
                {
                    spriteBatch.Draw(texture, rect, Microsoft.Xna.Framework.Color.Yellow);
                }
            }
            
            // Draw detailed collision rectangles for cold entities in orange
            foreach (var entity in _cold)
            {
                foreach (var rect in entity.CollisionRectangles)
                {
                    spriteBatch.Draw(texture, rect, Microsoft.Xna.Framework.Color.Orange);
                }
            }
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
    Player,
    Mobile,
    JetFighter,
    WeaponSpawn
}
