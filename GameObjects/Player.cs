using Hands.Core;
using Hands.Core.Animation;
using Hands.Core.Managers.Collision;
using Hands.Core.Sprites;
using Hands.GameObjects.Weapons;
using Hands.Sprites;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Linq;

namespace Hands.GameObjects;
internal class Player : IGameObject, IMapPosition, ICollision
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

    #region Collision

    public Rectangle Clayton =>
        new Rectangle((MapPosition - Size48.Size).ToPoint(), Size48.Point);

    public Rectangle[] CollisionRectangles =>
        new List<Point>
        {
            new Point(23, 0),
            new Point(24, 0),
            new Point(20, 2),
            new Point(27, 2),
            new Point(17, 6),
            new Point(30, 6),
            new Point(15, 9),
            new Point(33, 10),
            new Point(13, 13),
            new Point(35, 13),
            new Point(11, 16),
            new Point(37, 16),
            new Point(9, 19),
            new Point(39, 19),
            new Point(7, 20),
            new Point(41, 21),
            new Point(5, 22),
            new Point(43, 23),
            new Point(3, 24),
            new Point(45, 25),
            new Point(1, 26),
            new Point(47, 27),
            new Point(1, 30),
            new Point(4, 30),
            new Point(8, 30),
            new Point(39, 30),
            new Point(43, 30),
            new Point(47, 30),
            new Point(8, 33),
            new Point(39, 33),
            new Point(8, 36),
            new Point(39, 36),
            new Point(8, 40),
            new Point(39, 40),
            new Point(8, 43),
            new Point(39, 43),
            new Point(8, 45),
            new Point(39, 45),
            new Point(11, 47),
            new Point(12, 47),
            new Point(23, 47),
            new Point(25, 47),
            new Point(35, 47),
            new Point(36, 47)
        }
        .Select(x => new Rectangle((MapPosition - Size48.Size).ToPoint() + x.ToVector2().ToPoint(), new Point(1, 1)))
        .ToArray();        

    public CollisionType CollisionType => CollisionType.Player;

    public bool IsHot => true;

    private void DrawCollisionBox(SpriteBatch spriteBatch)
    {
        Texture2D texture = spriteBatch.BlankTexture();
        if (Global.DebugShowCollisionBoxes)
        {
            foreach (var r in CollisionRectangles)
            {
                spriteBatch.Draw(texture, r, Color.Yellow);
            }            
        }
    }

    public void OnCollide(ICollision other)
    {
        throw new NotImplementedException();
    }

    #endregion


    public Vector2 MapPosition          { get; set; } = Global.World.GlobalPlayerPosition;
    public float MovementSpeed          { get; set; } = 0.35f;

    public Texture2D Texture            { get; private set; }
    public Dictionary<int, SpriteFrame> Frames 
                                        { get; private set; } = new Dictionary<int, SpriteFrame>();

    // Weapons
    public IWeapon MainWeapon           { get; set; }

}
