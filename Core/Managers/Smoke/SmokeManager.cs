using Hands.Sprites;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hands.Core.Managers.Smoke;
public class SmokeManager : ILoadContent, IUpdate, IDraw
{
    private readonly List<Smoke> _particles = new();

    public void Register(SmokeAreaInfo info)
    {
        // Optimization: Pre-allocate capacity if needed
        if (_particles.Capacity < _particles.Count + info.Count)
            _particles.Capacity = _particles.Count + info.Count;

        for (int i = 0; i < info.Count; i++)
        {
            // Generate a random position within the given area
            float x = info.Area.X + Random.Shared.NextSingle() * info.Area.Width;
            float y = info.Area.Y + Random.Shared.NextSingle() * info.Area.Height;
            var position = new Vector2(x, y);

            var smoke = new Smoke(position, TimeSpan.FromSeconds(info.StartDelay));
            _particles.Add(smoke);
        }
    }

    public void LoadContent(ContentManager contentManager)
    {
        Sprite.LoadContent(contentManager);
    }

    public void Update(GameTime gameTime)
    {
        //
        Parallel.ForEach(_particles, smoke =>
        {
            smoke.Update(gameTime);
        });
        _particles.RemoveAll(s => s.IsComplete);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Parallel.ForEach(_particles, smoke =>
        {
            smoke.Draw(spriteBatch);
        });
    }

    //
    // Properties
    //
    public SmokeSprite Sprite { get; init; } = new();

}

public record SmokeAreaInfo(Rectangle Area, int Count, float StartDelay);
// TODO: In the future, there might be SmokeEmitterInfo for things like rockets

public class SmokeSprite : ILoadContent
{
    public void LoadContent(ContentManager contentManager)
    {
        Texture = contentManager.Load<Texture2D>("Smoke");
        Frames = SpriteHelper.CreateFramesFromTexture(Texture, new Point(200, 200));
    }

    public Texture2D Texture { get; set; }
    public Dictionary<int, SpriteFrame> Frames { get; private set; }

}

