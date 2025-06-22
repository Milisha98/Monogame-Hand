using Hands.Core;
using Hands.Core.Managers.Collision;
using Hands.Core.Managers.Explosion;
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

    #region Events

    public void OnCollide(ICollision other)
    {
        var explosionInfo = new ExplosionInfo(MapPosition, 8);
        Global.World.ExplosionManager.Register(explosionInfo);
        Global.World.ProjectileManager.Unregister(this);
    }

    #endregion

    private ProjectileType ProjectileType => _info.ProjectileType;
    public Vector2 MapPosition { get; private set; }

    public Rectangle Clayton => new Rectangle((MapPosition - Size8.Center).ToPoint(), Size8.Point);

    public Rectangle[] CollisionRectangles => [ Clayton ];

    public CollisionType CollisionType => _info.CollisionType;
    public bool IsHot => true; // Projectiles are always active

}
