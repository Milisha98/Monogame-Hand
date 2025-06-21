using Hands.Core;
using Hands.Core.Animation;
using Hands.Core.Managers.Collision;
using Hands.Core.Sprites;
using Hands.GameObjects.Weapons;
using Hands.Sprites;

using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace Hands.GameObjects;
internal class Player : IGameObject, IMapPosition
{    
    private readonly Tween _startTween;

    private ContentManager _contentManager;
    private Texture2D _texture;
    private Dictionary<int, SpriteFrame> _frames;

    private Vector2 _shadowOffset = new Vector2(0, 0);
    private float _height = 0;
    private float _zoom = 0.8f;

    private List<IWeapon> Weapons = [];

    public Player()
    {
        _startTween = new Tween(TimeSpan.FromSeconds(1f));
        MainWeapon = new DefaultLaser();
    }

    public void LoadContent(ContentManager contentManager)
    {
        _contentManager = contentManager;
        _texture = contentManager.Load<Texture2D>("Player");
        _frames = SpriteHelper.CreateFramesFromTexture(_texture, Size48.Point);
        MainWeapon.LoadContent(contentManager);
    }

    public void Update(GameTime gameTime)
    {
        // Update the Rise off the ground animation
        if (_startTween.IsComplete == false)
        {
            UpdateRiseOffGroundAnimation(gameTime);
            return;
        }

        UpdateInput(gameTime);

        // Weapons
        MainWeapon.Update(gameTime);
    }

    private void UpdateInput(GameTime gameTime)
    {
        // Control the Player movement and actions here
        var move = KeyboardController.CheckInput();
        if (move == Vector2.Zero) return;
        move.Normalize();
        float speed = MovementSpeed * gameTime.ElapsedGameTime.Milliseconds;
        var proposedMapPosition = MapPosition + (move * speed);
        ProposedLocation = new Rectangle((proposedMapPosition - Size48.Size).ToPoint(), Size48.Point);
        if (Global.World.CollisionManager.IsClaytonCollision(ProposedLocation) == CollisionType.None)
        {
            MapPosition = proposedMapPosition;
        }
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

        MainWeapon.Draw(spriteBatch);

        DrawCollisionBox(spriteBatch);
    }

    private void DrawCollisionBox(SpriteBatch spriteBatch)
    {
        Texture2D texture = spriteBatch.BlankTexture();
        if (Global.DebugShowCollisionBoxes)
        {
            var location = (MapPosition - Size48.Size).ToPoint();
            var rectangle = new Rectangle(location, Size48.Point);
            spriteBatch.Draw(texture, rectangle, Color.Yellow);
        }
    }

    public Vector2 MapPosition          { get; set; } = Global.World.GlobalPlayerPosition;
    public float MovementSpeed          { get; set; } = 0.35f;
    public Rectangle ProposedLocation   { get; private set; }

    // Weapons
    public IWeapon MainWeapon           { get; set; }
    
}
