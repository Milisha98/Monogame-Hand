using Hands.Core.Managers.Collision;

namespace Hands.Core;
public interface ICollision
{
    public Rectangle Clayton { get; }
    public Rectangle[] CollisionRectangles { get; }
    public CollisionType CollisionType { get; }
    public bool IsHot { get; }
    public bool IsSmoker { get; }
    public void OnCollide(ICollision other);

}
