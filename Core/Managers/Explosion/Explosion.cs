using Hands.Core.Animation;
using Hands.Core.Sprites;

namespace Hands.Core.Managers.Explosion;

public class Explosion : IUpdate, IDraw, IMapPosition
{
    public const float Duration = 0.5f; // Default duration for explosion animations

    private Tween _tween;
    private Color _color;
    private float _scale = 1f;
    private float _rotation;

    public void Activate(ExplosionInfo info)
    {
        MapPosition = info.MapPosition;
        _tween = new Tween(TimeSpan.FromSeconds(Duration));
        _rotation = Random.Shared.NextSingle() * MathF.Tau;
        _scale = (float)(info.Size / 64f) * 3;
        _color = Color.White;
        Frame = 0;
    }

    public void Update(GameTime gameTime)
    {
        if (IsComplete) return;

        float percent = _tween.Update(gameTime);
        Frame = (int)(percent * Sprite.Frames.Count);

        int alpha = Math.Max(125, (int)(255 * (1f - percent)));
        _color = new Color(255, 255, 255, alpha);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsComplete) return;

        // Draw the explosion centered at MapPosition
        spriteBatch.Draw(Sprite.Texture, MapPosition, Sprite.Frames[Frame].SourceRectangle, _color, _rotation, Size64.Center, _scale, SpriteEffects.None, 0);
    }

    public bool IsComplete => _tween.IsComplete;
    public Vector2 MapPosition { get; private set; }
    public Vector2 Center => MapPosition + Size64.Center;
    public int Frame { get; private set; } = 0;
    internal ExplosionSprite Sprite => Global.World.ExplosionManager.Sprite;
}


