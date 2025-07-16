using System;

namespace Hands.Core.Animation;

public class SinTween
{
    private readonly TimeSpan _duration;
    private TimeSpan _elapsedTime;
    private bool _hasRestarted;
    private bool _hasFiredPeakEvent;
    private bool _hasFiredTroughEvent;

    public delegate void PeakReachedHandler();
    public event PeakReachedHandler OnPeakReached;
    
    public delegate void TroughReachedHandler();
    public event TroughReachedHandler OnTroughReached;

    public SinTween(TimeSpan duration)
    {
        _duration = duration;
        _elapsedTime = TimeSpan.Zero;
        _hasRestarted = false;
        _hasFiredPeakEvent = false;
        _hasFiredTroughEvent = false;
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
            _hasFiredTroughEvent = false;
            progress = 0f;
        }
        else if (progress >= 1f)
        {
            progress = 1f;
        }

        // Map progress to angle range: 0 to 2π (full cycle)
        // This creates a sine wave that goes from 0 to 1 to 0 to -1 to 0
        float angle = 2f * MathF.PI * progress;
        
        // Check if we've reached the peak (π/2 or 90 degrees)
        // Peak occurs at progress = 0.25 (quarter of cycle)
        if (!_hasFiredPeakEvent && progress >= 0.25f)
        {
            _hasFiredPeakEvent = true;
            OnPeakReached?.Invoke();
        }
        
        // Check if we've reached the trough (3π/2 or 270 degrees)
        // Trough occurs at progress = 0.75 (three-quarters of cycle)
        if (!_hasFiredTroughEvent && progress >= 0.75f)
        {
            _hasFiredTroughEvent = true;
            OnTroughReached?.Invoke();
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
        _hasFiredTroughEvent = false;
    }

    /// <summary>
    /// Gets the current angle in radians (0 to 2π)
    /// </summary>
    public float CurrentAngle
    {
        get
        {
            float progress = (float)_elapsedTime.TotalMilliseconds / (float)_duration.TotalMilliseconds;
            if (progress >= 1f) progress = 1f;
            return 2f * MathF.PI * progress;
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
