using Hands.Core.Sprites;
using Hands.GameObjects.Enemies.Turret;
using Hands.Sprites;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

