using Hands.Core.Animation;
using Hands.Core.Sprites;

namespace Hands.Core.Managers.Smoke;

public class Smoke : IUpdate, IDraw, IMapPosition
{
    public const float Duration = 5.0f; // Default duration for Smoke animations

    private Tween _startTween;
    private Tween _durationTween;
    private float _scale = 1f;
    private float _rotation;
    private Vector2 _movement;
    private int _tint;

    public void Activate(Vector2 mapPosition, TimeSpan startDelay)
    {
        var rnd = new Random();

        MapPosition = mapPosition;
        _startTween = new Tween(startDelay);                        // Initial delay before the smoke starts
        _durationTween = new Tween(TimeSpan.FromSeconds(Duration));
        _rotation = rnd.NextSingle() * MathF.Tau;
        _scale = (float)(rnd.NextDouble() * 0.25 + 0.1);
        _tint = 200;

        float velocity = (float)rnd.NextDouble();
        var deltaX = (float)Math.Sin(_rotation) * velocity;
        var deltaY = (float)-Math.Cos(_rotation) * velocity;
        _movement = new Vector2(deltaX, deltaY);
    }

    public void Update(GameTime gameTime)
    {
        if (IsComplete) return;

        if (_startTween.IsComplete == false)
        {
            var pct = _startTween.Update(gameTime);
            return;
        }

        float percent = _durationTween.Update(gameTime);
        _tint = (int)(200 * (1 - percent)); // Fade out the smoke
        MapPosition += _movement * (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 100);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsComplete) return;
        if (_startTween.IsComplete == false) return;

        // Draw the smoke using the first frame, centered
        spriteBatch.Draw(Sprite.Texture, MapPosition, Sprite.Frames[0].SourceRectangle, Color, _rotation, Size200.Center, _scale, SpriteEffects.None, 0);
    }
  
    public Color Color => new Color(_tint, _tint, _tint, _tint);
    public bool IsComplete => _durationTween.IsComplete;
    public Vector2 MapPosition { get; private set;  }
    public Vector2 Center => MapPosition + Size200.Center;
    internal SmokeSprite Sprite => Global.World.SmokeManager.Sprite;
}


