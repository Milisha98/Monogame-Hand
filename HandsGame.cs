using Hands.Core;
using Microsoft.Xna.Framework.Input;

namespace Hands;

public class HandsGame : Game
{
    public HandsGame()
    {
        Global.Graphics = new();
        Global.Graphics.Initialize(this);

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Core.Tiles.TiledReader.Load();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        Global.Scene.LoadContent(Content);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        Global.Scene.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {        
        Global.Graphics.Clear();
        Global.Scene.Draw(Global.Graphics.SpriteBatch);
        Global.Graphics.DrawAndScaleFromBackBuffer();
        base.Draw(gameTime);
    }
}
