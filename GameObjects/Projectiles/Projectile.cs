using Hands.Core;
using Hands.Core.Managers.Collision;
using Hands.Core.Sprites;

namespace Hands.GameObjects.Projectiles;
internal class Projectile : IUpdate, IDraw, IMapPosition, ICollision
{
    private readonly ProjectileInfo _info;

    public Projectile(ProjectileInfo projectileInfo)
    {
        _info = projectileInfo;
        MapPosition = projectileInfo.MapPosition;
    }

    public void Update(GameTime gameTime)
    {
        Vector2 v = _info.Vector;
        MapPosition += v;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var sprite = Global.World.ProjectileManager.Sprite[ProjectileType];
        spriteBatch.Draw(sprite.Texture, MapPosition, sprite.Frame.SourceRectangle, Color.White);
    }

    private ProjectileType ProjectileType => _info.ProjectileType;
    public Vector2 MapPosition { get; private set; }

    public Rectangle Clayton => new Rectangle((MapPosition - Size8.Center).ToPoint(), Size8.Point);

    public Rectangle[] CollisionRectangles => [ Clayton ];

    public CollisionType CollisionType => CollisionType.Projectile;
}
