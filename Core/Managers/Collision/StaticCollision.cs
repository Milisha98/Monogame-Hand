namespace Hands.Core.Managers.Collision;
public record StaticCollision : ICollision
{
    public StaticCollision(Rectangle claytons, Rectangle[] collisions, CollisionType collisionType)
    {
        Clayton = claytons;
        CollisionType = collisionType;
        if (collisions?.Length == 0)
        {
            CollisionRectangles = [claytons];
        }
    }

    public Rectangle Clayton { get; init; }

    public Rectangle[] CollisionRectangles { get; init; }

    public CollisionType CollisionType { get; set; } = CollisionType.Wall;

    public bool IsHot { get; set; } = false;
    public void OnCollide(ICollision other)
    {
        // Static collisions do not handle collisions, they are just barriers.        
    }
}