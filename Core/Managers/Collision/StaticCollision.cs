namespace Hands.Core.Managers.Collision;
public record StaticCollision : ICollision
{
    public StaticCollision(Rectangle claytons, Rectangle[] collisions = null)
    {
        Clayton = claytons;
        if (collisions?.Length == 0)
        {
            CollisionRectangles = [claytons];
        }
    }

    public Rectangle Clayton { get; init; }

    public Rectangle[] CollisionRectangles { get; init; }

    public CollisionType CollisionType { get; set; } = CollisionType.Wall;
}