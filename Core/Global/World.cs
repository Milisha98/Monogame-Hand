using Hands.GameObjects.Enemies.Turret;

namespace Hands.Core;
internal class World
{
    public Camera2D Camera { get; set; } = new Camera2D();

    public WorldMap Map { get; set; } = new();

    // Managers
    public TurretManager TurretManager { get; set; } = new TurretManager();
}
