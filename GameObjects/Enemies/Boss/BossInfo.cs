namespace Hands.GameObjects.Enemies.Boss;
public record BossInfo(int X, int Y, float WakeDistance);

public record KeyInfo(int X, int Y, int Width, int Height = 48, char? Key1 = null, char? Key2 = null);
