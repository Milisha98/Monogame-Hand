using Hands.Core;
using Hands.GameObjects.Tiles;
using Microsoft.Xna.Framework.Content;


namespace Hands.Scenes;
internal class DefaultScene : IScene
{
    const int   MouseMoveGutter = 200;
    const float MouseZoomSpeed  = 0.01f;

    const int   CameraOffsetY = 200;

    // Tiles
    private readonly Tiler _tiler = new();

    public ScenesEnum Scene => ScenesEnum.Default;

    public void LoadContent(ContentManager contentManager)
    {
        _tiler.LoadContent(contentManager);
        Global.World.Camera.Position = Global.World.GlobalPlayerPosition - new Vector2(0, CameraOffsetY);

        Global.World.Player = new();
        Global.World.Player.LoadContent(contentManager);
        Global.World.CollisionManager.Register(Global.World.Player);

        Global.World.TurretManager.LoadContent(contentManager);
        Global.World.SideGunManager.LoadContent(contentManager);
        Global.World.MobileManager.LoadContent(contentManager);
        Global.World.JetFighterManager.LoadContent(contentManager);

        Global.World.ProjectileManager.LoadContent(contentManager);
        Global.World.ExplosionManager.LoadContent(contentManager);
        Global.World.SmokeManager.LoadContent(contentManager);
        Global.World.WeaponSpawnManager.LoadContent(contentManager);

        // Load Boss content if Boss exists
        Global.World.Boss.LoadContent(contentManager);

    }
    public void Update(GameTime gameTime)
    {
        Global.World.Player.Update(gameTime);

        Global.World.TurretManager.Update(gameTime);
        Global.World.SideGunManager.Update(gameTime);
        Global.World.MobileManager.Update(gameTime);
        Global.World.JetFighterManager.Update(gameTime);

        Global.World.SleepManager.Update(gameTime);
        Global.World.ProjectileManager.Update(gameTime);
        Global.World.CollisionManager.Update(gameTime);
        Global.World.ExplosionManager.Update(gameTime);
        Global.World.SmokeManager.Update(gameTime);
        Global.World.WeaponSpawnManager.Update(gameTime);

        // Update Boss
        Global.World.Boss.Update(gameTime);

        // Move Camera to lock on the Player
        Global.World.Camera.Position = new(Global.World.Camera.Position.X, Global.World.Player.MapPosition.Y - CameraOffsetY);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(transformMatrix: Global.World.Camera.ViewMatrix);

        _tiler.Draw(spriteBatch);

        Global.World.TurretManager.Draw(spriteBatch);
        Global.World.SideGunManager.Draw(spriteBatch);
        Global.World.MobileManager.Draw(spriteBatch);
        Global.World.JetFighterManager.Draw(spriteBatch);

        Global.World.ProjectileManager.Draw(spriteBatch);
        Global.World.Player.Draw(spriteBatch);
        Global.World.ExplosionManager.Draw(spriteBatch);
        Global.World.SmokeManager.Draw(spriteBatch);
        Global.World.WeaponSpawnManager.Draw(spriteBatch);

        // Draw Boss
        Global.World.Boss.Draw(spriteBatch);

        if (Global.DebugShowClaytonCollisionBoxes || Global.DebugShowCollisionBoxes)
        {
            Global.World.CollisionManager.Draw(spriteBatch);
        }

        spriteBatch.End();
    }
    public void OnChangeSceneStart()
    {
    }

    public void OnChangeSceneStop()
    {
    }
}
