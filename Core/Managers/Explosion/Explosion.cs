using Hands.Core.Animation;
using Hands.Core.Sprites;

namespace Hands.Core.Managers.Explosion;

public class Explosion : IUpdate, IDraw
{
    public const float Duration = 0.5f; // Default duration for explosion animations

    private readonly Tween _tween;
    private Color _color = Color.White;
    private float _scale = 1f;
    private float _rotation;

    public Explosion(ExplosionInfo info)
    {
        MapPosition = info.MapPosition;
        _tween = new Tween(TimeSpan.FromSeconds(Duration));
        _rotation = Random.Shared.NextSingle() * MathF.Tau;
        _scale = (float)(info.Size / 64f) * 3;
    }

    public void Update(GameTime gameTime)
    {
        if (IsComplete) return;

        float percent = _tween.Update(gameTime);
        Frame = (int)(percent * ((float)Sprite.Frames.Count));

        int alpha = Math.Max(125, (int)(255 * (1f - percent)));
        _color = new Color(255, 255, 255, alpha);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsComplete) return;

        // Draw the explosion using the first frame
        spriteBatch.Draw(Sprite.Texture, MapPosition + Size64.Center, Sprite.Frames[Frame].SourceRectangle, _color, _rotation, Size64.Center, _scale, SpriteEffects.None, 0);
    }

    public bool IsComplete => _tween.IsComplete;
    public Vector2 MapPosition { get; }
    public int Frame { get; private set; } = 0;
    internal ExplosionSprite Sprite => Global.World.ExplosionManager.Sprite;
}


