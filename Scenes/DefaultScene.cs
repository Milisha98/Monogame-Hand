using Hands.Core;
using Hands.GameObjects.Tiles;
using Microsoft.Xna.Framework.Content;


namespace Hands.Scenes;
internal class DefaultScene : IScene
{
    const int   MouseMoveGutter = 200;
    const float MouseZoomSpeed  = 0.01f;

    // Tiles
   private readonly Tiler _tiler = new();


    public ScenesEnum Scene => ScenesEnum.Default;

    public void LoadContent(ContentManager contentManager)
    {
        _tiler.LoadContent(contentManager);
        Global.World.Camera.Position = Global.World.GlobalPlayerPosition;

        Global.World.Player = new();
        Global.World.Player.LoadContent(contentManager);

    }
    public void Update(GameTime gameTime)
    {
        MoveCamera();

        MouseController.Update();
        Global.World.Player.Update(gameTime);
        Global.World.TurretManager.Update(gameTime);
        Global.World.SleepManager.Update(gameTime);
    }

    private static void MoveCamera()
    {
        // Move Camera
        var move = KeyboardController.CheckInput();

        // Mouse Bounds
        var mousePosition = MouseController.MousePosition;
        if (mousePosition.X < Global.World.Camera.Bounds.Left + MouseMoveGutter) move += new Vector2(-1, 0);
        if (mousePosition.X > Global.World.Camera.Bounds.Right - MouseMoveGutter) move += new Vector2(1, 0);
        if (mousePosition.Y < Global.World.Camera.Bounds.Top + MouseMoveGutter) move += new Vector2(0, -1);
        if (mousePosition.Y > Global.World.Camera.Bounds.Bottom - MouseMoveGutter) move += new Vector2(0, 1);

        // Cannot move outside of map
        if (move.X < 0f && Global.World.Camera.Position.X < Global.Graphics.RenderWidth / 2) move = Vector2.Zero;
        if (move.X > 0f && Global.World.Camera.Position.X > Global.Graphics.RenderWidth) move = Vector2.Zero;
        if (move.Y < 0f && Global.World.Camera.Position.Y < Global.Graphics.RenderHeight / 2) move = Vector2.Zero;
        if (move.Y > 0f && Global.World.Camera.Position.Y > Global.Graphics.RenderHeight + 200) move = Vector2.Zero;

        if (move != Vector2.Zero) move.Normalize();
        Global.World.Camera.Position += move * Global.CameraSpeed;
    }

    private static void ZoomCamera()
    {
        // Mouse Zoom
        float zoom = Global.World.Camera.Zoom;
        if (MouseController.MouseWheel == MouseWheelEnum.Up) zoom += MouseZoomSpeed;
        if (MouseController.MouseWheel == MouseWheelEnum.Down) zoom -= MouseZoomSpeed;
        zoom = Math.Clamp(zoom, 0.5f, 1f);
        Global.World.Camera.Zoom = zoom;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(transformMatrix: Global.World.Camera.ViewMatrix);

        _tiler.Draw(spriteBatch);
        Global.World.TurretManager.Draw(spriteBatch);
        Global.World.Player.Draw(spriteBatch);

        spriteBatch.End();
    }
    public void OnChangeSceneStart()
    {
    }

    public void OnChangeSceneStop()
    {
    }
}
