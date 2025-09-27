namespace Hands.GameObjects.Enemies.Boss;
public record BossInfo(int X, int Y, float WakeDistance);

public record KeyInfo(int X, int Y, int Width, int Height = 48, string Key1 = null, string Key2 = null);
