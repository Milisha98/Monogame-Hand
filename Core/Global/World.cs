using Hands.Core.Managers;
using Hands.Core.Sprites;
using Hands.GameObjects;
using Hands.GameObjects.Enemies.Turret;

namespace Hands.Core;
internal class World
{
    public float GlobalWakeDistance => 750f;
    public Vector2 GlobalPlayerPosition => new Vector2(Global.TileDimension * 18, Global.TileDimension * 190);

    public Camera2D Camera { get; set; } = new Camera2D();
    public WorldMap Map { get; set; } = new();
    public Player Player { get; set; }

    // Managers
    public TurretManager TurretManager { get; set; } = new TurretManager();
    public SleepManager SleepManager { get; set; } = new SleepManager();
}
