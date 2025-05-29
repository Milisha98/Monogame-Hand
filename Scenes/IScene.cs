using Hands.Core;

namespace Hands.Scenes;

internal interface IScene : IGameObject
{
    public void OnChangeSceneStart();
    public void OnChangeSceneStop();
    public ScenesEnum Scene { get; }
}
