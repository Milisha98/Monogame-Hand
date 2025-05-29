using Hands.Scenes;

namespace Hands.Core;
internal static class Global
{
    public const int TileDimension = 32;
    public const int CameraSpeed = 5;

    public static SceneManager Scene    { get; set; }   = new(ScenesEnum.Default);
    public static Graphics Graphics     { get; set; }   = new();
    public static World World           { get; set; }   = new();
}
