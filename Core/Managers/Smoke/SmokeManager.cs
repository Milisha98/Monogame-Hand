using Hands.Sprites;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hands.Core.Managers.Smoke;
public class SmokeManager : ILoadContent, IUpdate, IDraw
{
    private List<Smoke> _objectCache = [];
    private List<Smoke> _particles   = [];

    public SmokeManager()
    {
        const int initialCapacity = 200; // Initial capacity for the smoke particles
        _particles      = new List<Smoke>(initialCapacity);
        _objectCache    = new List<Smoke>(initialCapacity);
        for (int i = 0; i < initialCapacity; i++)
        {
            _objectCache.Add(new Smoke());
        }
    }

    public void Register(SmokeAreaInfo info)
    {
        // Move the smoke particles from the cache to the active list
        for (int i = 0; i < info.Count && _objectCache.Count > 0; i++)
        {
            // Generate a random position within the given area
            float x = info.Area.X + Random.Shared.NextSingle() * info.Area.Width;
            float y = info.Area.Y + Random.Shared.NextSingle() * info.Area.Height;
            var position = new Vector2(x, y);

            var smoke = _objectCache[0];
            _objectCache.RemoveAt(0);
            smoke.Activate(position, TimeSpan.FromSeconds(info.StartDelay));
            _particles.Add(smoke);
        }
    }

    public void LoadContent(ContentManager contentManager)
    {
        Sprite.LoadContent(contentManager);
    }

    public void Update(GameTime gameTime)
    {
        Parallel.ForEach(_particles, smoke =>
        {
            smoke.Update(gameTime);
        });
        // Move completed particles back to the cache
        _objectCache.AddRange(_particles.FindAll(s => s.IsComplete));
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

