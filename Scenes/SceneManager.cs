using Hands.Core;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Hands.Scenes;
internal class SceneManager : IGameObject
{
    private readonly IScene _defaultScene = new DefaultScene();
    private IScene _currentScene = null;

    public SceneManager(ScenesEnum initialScene)
    {
        ChangeScene(initialScene);
    }

    public void ChangeScene(ScenesEnum scene)
    {
        _currentScene?.OnChangeSceneStop();
        _currentScene = scene switch
        {
            ScenesEnum.Default  => _defaultScene,
            _                   => throw new ArgumentOutOfRangeException(nameof(scene), scene, null)
        };
        _currentScene.OnChangeSceneStart();
    }

    public void LoadContent(ContentManager contentManager)
    {
        _defaultScene.LoadContent(contentManager);
    }

    public void Update(GameTime gameTime)
    {
        _currentScene?.Update(gameTime);
    }
    public void Draw(SpriteBatch spriteBatch)
    {
        _currentScene?.Draw(spriteBatch);
    }

}
