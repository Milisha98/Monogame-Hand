using Hands.Core;
using Hands.Core.Animation;
using Hands.Core.Sprites;
using Hands.Sprites;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace Hands.GameObjects;
internal class Player : IGameObject, IMapPosition
{
    private readonly Tween _startTween;

    private Texture2D _texture;
    private Dictionary<int, SpriteFrame> _frames;

    private Vector2 _shadowOffset = new Vector2(0, 0);
    private float _height = 0;
    private float _zoom = 0.8f;

    public Player()
    {
        _startTween = new Tween(TimeSpan.FromSeconds(1f));
    }

    public void LoadContent(ContentManager contentManager)
    {
        _texture = contentManager.Load<Texture2D>("Player");
        _frames = SpriteHelper.CreateFramesFromTexture(_texture, Size48.Point);
    }

    public void Update(GameTime gameTime)
    {
        // Update the Rise off the ground animation
        if (_startTween.IsComplete == false)
        {
            UpdateRiseOffGroundAnimation(gameTime);
            return;
        }

        // TODO: Control the Player movement and actions here
    }

    private void UpdateRiseOffGroundAnimation(GameTime gameTime)
    {       
        float pct = _startTween.Update(gameTime);
        _zoom = 0.8f + (0.2f * pct);
        float offset = 12f * pct;
        _shadowOffset = new Vector2(offset, offset);

    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_texture, MapPosition - Size48.Center + _shadowOffset, _frames[1].SourceRectangle, Color.White, 0f, Size48.Center, _zoom, SpriteEffects.None, _height);
        spriteBatch.Draw(_texture, MapPosition - Size48.Center, _frames[0].SourceRectangle, Color.White, 0f, Size48.Center, _zoom, SpriteEffects.None, 0f);
    }

    public Vector2 MapPosition { get; set; } = Global.World.GlobalPlayerPosition;
}
