using System.Linq;

namespace Hands.Core;
internal class Graphics
{

    public GraphicsDeviceManager GraphicsManager;
    public Viewport DefaultView;

    public float DisplayRatio = 16f / 9f;

    public int ScreenWidth = 1760;
    public int ScreenHeight = 990;

    public int RenderWidth = 1280;
    public int RenderHeight = 720;

    public bool IsShadowOn = true;

    public RenderTarget2D RenderTarget { get; private set; }
    public SpriteBatch SpriteBatch { get; set; }
    public int ViewWidth { get => ScreenWidth / 2; }
    public GraphicsDevice GraphicsDevice { get => GraphicsManager.GraphicsDevice; }

    public float Scale { get; set; } = 1f;

    public void Initialize(HandsGame game)
    {
        // Detect Screen Size
        var displayMode = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes
                            .OrderByDescending(x => x.Width)
                            .FirstOrDefault(dm => dm.AspectRatio == Global.Graphics.DisplayRatio &&
                                                  dm.Width < GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width &&
                                                  dm.Height < GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                            ?? GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.First();
        ScreenWidth = displayMode.Width;
        ScreenHeight = displayMode.Height;

        GraphicsDeviceManager graphics = new(game)
        {
            PreferredBackBufferWidth = ScreenWidth,
            PreferredBackBufferHeight = ScreenHeight
        };
        graphics.ApplyChanges();
        Global.Graphics.GraphicsManager = graphics;

        DefaultView = GraphicsDevice.Viewport;

        RenderTarget = new RenderTarget2D(GraphicsDevice, ScreenWidth, ScreenHeight);
        SpriteBatch = new SpriteBatch(GraphicsDevice);
    }

    public void Clear()
    {
        Global.Graphics.Scale = 1f / (((float)Global.Graphics.RenderHeight) / GraphicsDevice.Viewport.Height);
        GraphicsDevice.SetRenderTarget(RenderTarget);
        GraphicsDevice.Clear(Color.Black);
    }

    public void DrawAndScaleFromBackBuffer()
    {
        // Display the Back-Buffer to the screen with a scale
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        SpriteBatch.Begin();
        SpriteBatch.Draw(RenderTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, Global.Graphics.Scale, SpriteEffects.None, 0f);
        SpriteBatch.End();
    }
}
