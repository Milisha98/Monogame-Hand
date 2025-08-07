namespace Hands.Core.Managers.Smoke;

/// <summary>
/// Information for creating continuous smoke trails (like jet fighter con-trails)
/// </summary>
public record SmokeTrailInfo(Vector2 Position, float ParticleRate, float StartDelay = 0f);
