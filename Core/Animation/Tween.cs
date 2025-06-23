using System;

namespace Hands.Core.Animation;
public class Tween
{
    private readonly TimeSpan _duration;
    private TimeSpan _elapsedTime;
    private bool _isCompleted;

    public delegate void TweenCompletedHandler();
    public event TweenCompletedHandler OnCompleted;

    public Tween(TimeSpan duration)
    {
        this._duration = duration;
        _elapsedTime = TimeSpan.Zero;
        _isCompleted = false;
    }

    public float Update(GameTime gameTime)
    {
        if (IsActive == false) return 0f;
        if (_isCompleted) return 1f;

        _elapsedTime += gameTime.ElapsedGameTime;

        if (_elapsedTime >= _duration)
        {
            _isCompleted = true;
            OnCompleted?.Invoke();
            return 1f;
        }

        return (float)_elapsedTime.TotalMilliseconds / (float)_duration.TotalMilliseconds;
    }

    public void Reset()
    {
        _elapsedTime = TimeSpan.Zero;
        _isCompleted = false;
    }

    public bool IsActive { get; set; } = true;
    public bool IsComplete => _isCompleted;
}