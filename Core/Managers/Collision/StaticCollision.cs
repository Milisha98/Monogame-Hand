namespace Hands.Core.Managers.Collision;
public record StaticCollision : ICollision
{
    public StaticCollision(Rectangle claytons, Rectangle[] collisions, CollisionType collisionType)
    {
        Clayton = claytons;
        CollisionType = collisionType;
        CollisionRectangles = collisions is { Length: > 0 } ? collisions : [claytons];
    }

    public Rectangle Clayton { get; init; }

    public Rectangle[] CollisionRectangles { get; init; }

    public CollisionType CollisionType { get; set; } = CollisionType.Wall;

    public bool IsHot { get; set; } = false;
    public bool ShouldRemoveOnCollision { get; set; } = false; // Static collisions are never removed
    public void OnCollide(ICollision other)
    {
        // Static collisions do not handle collisions, they are just barriers.        
    }
}