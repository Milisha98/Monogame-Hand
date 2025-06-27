using Hands.Core.Sprites;
using Hands.Sprites;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hands.Core.Managers.Explosion;
public class ExplosionManager : ILoadContent, IUpdate, IDraw
{
    private List<Explosion> _objectCache = [];
    private List<Explosion> _explosions  = [];

    public ExplosionManager()
    {
        const int maxExplosions = 20;       // Limit the number of explosions to prevent memory issues
        _objectCache = new List<Explosion>(maxExplosions);
        _explosions = new List<Explosion>(maxExplosions);
        for (int i = 0; i < maxExplosions; i++)
        {
            _objectCache.Add(new Explosion());
        }
    }

    public void Register(ExplosionInfo info)
    {
        Explosion explosion = null;
        if (_objectCache.Any())
        {
            explosion = _objectCache[0];
            _objectCache.RemoveAt(0);
        }
       
        if (explosion is null) explosion = new Explosion();

        explosion.Activate(info);
        _explosions.Add(explosion);
    }

    public void UnRegister(Explosion explosion)
    {
        _objectCache.Add(explosion);
        _explosions.Remove(explosion);
    }
    public void LoadContent(ContentManager contentManager)
    {
        Sprite.LoadContent(contentManager);
    }

    public void Update(GameTime gameTime)
    {
        // Update all explosions
        Parallel.ForEach(_explosions, explosion =>
        {
            explosion.Update(gameTime);
        });

        // Remove completed explosions
        _explosions.RemoveAll(e => e.IsComplete);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Parallel.ForEach(_explosions, explosion =>
        {
            explosion.Draw(spriteBatch);
        });
    }

    //
    // Properties
    //
    public ExplosionSprite Sprite { get; init; } = new();

}

public record ExplosionInfo(Vector2 MapPosition, int Size);

public class ExplosionSprite: ILoadContent
{
    public void LoadContent(ContentManager contentManager)
    {
        Texture = contentManager.Load<Texture2D>("Explosion");
        Frames = SpriteHelper.CreateFramesFromTexture(Texture, Size64.Point);
    }

    public Texture2D Texture { get; set; }
    public Dictionary<int, SpriteFrame> Frames { get; private set; }

}

