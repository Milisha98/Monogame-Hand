using System;

namespace Hands.Core.Animation;

public class SinTween
{
    private readonly TimeSpan _duration;
    private TimeSpan _elapsedTime;
    private bool _hasRestarted;
    private bool _hasFiredPeakEvent;

    public delegate void PeakReachedHandler();
    public event PeakReachedHandler OnPeakReached;

    public SinTween(TimeSpan duration)
    {
        _duration = duration;
        _elapsedTime = TimeSpan.Zero;
        _hasRestarted = false;
        _hasFiredPeakEvent = false;
    }

    /// <summary>
    /// Updates the tween and returns the current sin value (-1 to 1)
    /// </summary>
    /// <param name="gameTime">The game time</param>
    /// <returns>The current sin value from -1 to 1</returns>
    public float Update(GameTime gameTime)
    {
        if (!IsActive) return 0f;

        _elapsedTime += gameTime.ElapsedGameTime;

        // Calculate progress through the cycle (0 to 1)
        float progress = (float)_elapsedTime.TotalMilliseconds / (float)_duration.TotalMilliseconds;

        // Handle auto-restart
        if (progress >= 1f && AutoRestart)
        {
            _elapsedTime = TimeSpan.Zero;
            _hasRestarted = true;
            _hasFiredPeakEvent = false;
            progress = 0f;
        }
        else if (progress >= 1f)
        {
            progress = 1f;
        }

        // Map progress to angle range: -π/2 to π/2 to -π/2 (full cycle)
        // This creates a sine wave that goes from -1 (bottom) to 1 (top) to -1 (bottom)
        float angle = MathF.PI * progress - MathF.PI / 2f;
        
        // Check if we've reached the peak (π/2 or 90 degrees)
        // Peak occurs at progress = 0.5 (middle of cycle)
        if (!_hasFiredPeakEvent && progress >= 0.5f)
        {
            _hasFiredPeakEvent = true;
            OnPeakReached?.Invoke();
        }

        return MathF.Sin(angle);
    }

    /// <summary>
    /// Resets the tween to its initial state
    /// </summary>
    public void Reset()
    {
        _elapsedTime = TimeSpan.Zero;
        _hasRestarted = false;
        _hasFiredPeakEvent = false;
    }

    /// <summary>
    /// Gets the current angle in radians (-π/2 to π/2)
    /// </summary>
    public float CurrentAngle
    {
        get
        {
            float progress = (float)_elapsedTime.TotalMilliseconds / (float)_duration.TotalMilliseconds;
            if (progress >= 1f) progress = 1f;
            return MathF.PI * progress - MathF.PI / 2f;
        }
    }

    /// <summary>
    /// Gets the current progress through the cycle (0 to 1)
    /// </summary>
    public float CurrentProgress
    {
        get
        {
            float progress = (float)_elapsedTime.TotalMilliseconds / (float)_duration.TotalMilliseconds;
            return progress >= 1f ? 1f : progress;
        }
    }

    /// <summary>
    /// Whether the tween is active and updating
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the tween automatically restarts when it completes a cycle
    /// </summary>
    public bool AutoRestart { get; set; } = true;

    /// <summary>
    /// Whether the tween has completed at least one full cycle
    /// </summary>
    public bool HasCompleted => _elapsedTime >= _duration;

    /// <summary>
    /// Whether the tween has restarted at least once (only meaningful when AutoRestart is true)
    /// </summary>
    public bool HasRestarted => _hasRestarted;
}
