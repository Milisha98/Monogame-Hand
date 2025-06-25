using Hands.Core;
using Hands.Core.Animation;
using Hands.Core.Managers.Collision;
using Hands.Core.Sprites;
using Hands.GameObjects.Weapons;
using Hands.Sprites;

using Microsoft.Xna.Framework.Content;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Hands.GameObjects;
internal class Player : IGameObject, IMapPosition
{    
    private readonly Tween _startTween;
    private Vector2 _shadowOffset = new Vector2(0, 0);
    private float _height = 0;
    private float _zoom = 0.8f;

    public Player()
    {
        _startTween = new Tween(TimeSpan.FromSeconds(1f));
        MainWeapon = new DefaultWeapon(); // Default weapon
    }

    public void LoadContent(ContentManager contentManager)
    {
        Texture = contentManager.Load<Texture2D>("Player");
        Frames = SpriteHelper.CreateFramesFromTexture(Texture, Size48.Point);
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
        Rectangle proposedClayton = new Rectangle((proposedMapPosition - Size48.Size).ToPoint(), Size48.Point);
        var proposedCollision = new StaticCollision(proposedClayton, [], CollisionType.Player);
        var result = Global.World.CollisionManager.CheckClaytonsCollision(proposedCollision);
        if (result is null || Global.World.CollisionManager.IsCollisionWeCareAbout(CollisionType.Player, result.CollisionType) == false)
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
        spriteBatch.Draw(Texture, MapPosition - Size48.Center + _shadowOffset, Frames[1].SourceRectangle, Color.White, 0f, Size48.Center, _zoom, SpriteEffects.None, _height);
        MainWeapon.DrawShadow(spriteBatch);

        spriteBatch.Draw(Texture, MapPosition - Size48.Center, Frames[0].SourceRectangle, Color.White, 0f, Size48.Center, _zoom, SpriteEffects.None, 0f);
        MainWeapon.DrawWeapon(spriteBatch);

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

    public Texture2D Texture            { get; private set; }
    public Dictionary<int, SpriteFrame> Frames 
                                        { get; private set; } = new Dictionary<int, SpriteFrame>();

    // Weapons
    public IWeapon MainWeapon           { get; set; }
    
}
