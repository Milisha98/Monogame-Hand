using Hands.Core.Sprites;
using Hands.Sprites;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

namespace Hands.Core.Managers.Explosion;
public class ExplosionManager : ILoadContent, IUpdate, IDraw
{
    private readonly List<Explosion> _explosions = new();

    public void Register(ExplosionInfo explosion)
    {
        var newExplosion = new Explosion(explosion);
        _explosions.Add(newExplosion);
    }

    public void UnRegister(Explosion explosion)
    {
        _explosions.Remove(explosion);
    }
    public void LoadContent(ContentManager contentManager)
    {
        Sprite.LoadContent(contentManager);
    }

    public void Update(GameTime gameTime)
    {
        // Update all explosions and remove those that are finished
        for (int i = _explosions.Count - 1; i >= 0; i--)
        {
            var explosion = _explosions[i];
            explosion.Update(gameTime);
            if (explosion.IsComplete)
            {
                _explosions.RemoveAt(i);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var explosion in _explosions)
        {
            explosion.Draw(spriteBatch);
        }
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

