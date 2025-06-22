using Hands.Core.Managers;
using Hands.Core.Managers.Collision;
using Hands.Core.Managers.Explosion;
using Hands.Core.Managers.Smoke;
using Hands.GameObjects;
using Hands.GameObjects.Enemies.Turret;
using Hands.GameObjects.Projectiles;

namespace Hands.Core;
internal class World
{
    public float GlobalWakeDistance => 500f;
    public Vector2 GlobalPlayerPosition => new Vector2(Global.TileDimension * 18, Global.TileDimension * 196);

    public Camera2D Camera  { get; set; } = new Camera2D();
    public WorldMap Map     { get; set; } = new();
    public Player Player    { get; set; }

    // Managers
    public TurretManager        TurretManager       { get; set; } = new TurretManager();
    public SleepManager         SleepManager        { get; set; } = new SleepManager();
    public CollisionManager     CollisionManager    { get; set; } = new CollisionManager();
    public ProjectileManager    ProjectileManager   { get; set; } = new ProjectileManager();
    public ExplosionManager     ExplosionManager    { get; set; } = new ExplosionManager();
    public SmokeManager         SmokeManager        { get; set; } = new SmokeManager();
}
