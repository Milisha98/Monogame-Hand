using Hands.Core;
using Hands.Core.Animation;
using Hands.Core.Managers.Collision;
using Hands.Core.Managers.Explosion;
using Hands.Core.Managers.Smoke;
using Hands.Core.Sprites;
using Hands.GameObjects.Projectiles;
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
        // Don't update if player is dead
        if (!IsAlive) return;

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
        var claytonsMapPosition = proposedMapPosition - Size48.Size;
        Rectangle proposedClayton = Clayton.Move(claytonsMapPosition);

        // I can't get fine-grained collsiion detection working here.
        //var proposedCollisionRectangles = 
        //    CollisionRectangles
        //        .Select(r => r.Move(claytonsMapPosition))
        //        .ToArray();
        var proposedCollision = new StaticCollision(proposedClayton, [], CollisionType.Player);
        var result = Global.World.CollisionManager.CheckCollision(proposedCollision);
        if (result is null)
        {            
            MapPosition = proposedMapPosition;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"Player collision detected at {proposedMapPosition} with {result.CollisionType}");
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
        // Don't draw the player if they're dead
        if (!IsAlive) return;

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
        [
            new Rectangle(10, 17, 29, 29),
            new Rectangle(17, 4, 14, 13),
            new Rectangle(8, 20, 2, 26),
            new Rectangle(39, 24, 6, 7),
            new Rectangle(2, 25, 6, 6),
            new Rectangle(10, 46, 28, 1),
            new Rectangle(31, 12, 5, 5),
            new Rectangle(14, 10, 3, 7),
            new Rectangle(20, 2, 8, 2),
            new Rectangle(39, 31, 1, 15),
            new Rectangle(45, 27, 3, 4),
            new Rectangle(31, 9, 3, 3),
            new Rectangle(39, 21, 3, 3),
            new Rectangle(5, 22, 3, 3),
            new Rectangle(12, 13, 2, 4),
            new Rectangle(0, 27, 2, 4),
            new Rectangle(23, 0, 2, 2),
            new Rectangle(16, 6, 1, 4),
            new Rectangle(31, 6, 1, 3),
            new Rectangle(36, 14, 1, 3),
            new Rectangle(23, 47, 3, 1),
            new Rectangle(18, 3, 2, 1),
            new Rectangle(28, 3, 2, 1),
            new Rectangle(15, 8, 1, 2),
            new Rectangle(11, 15, 1, 2),
            new Rectangle(39, 19, 1, 2),
            new Rectangle(7, 20, 1, 2),
            new Rectangle(42, 22, 1, 2),
            new Rectangle(4, 23, 1, 2),
            new Rectangle(45, 25, 1, 2),
            new Rectangle(11, 47, 2, 1),
            new Rectangle(35, 47, 2, 1),
            new Rectangle(22, 1, 1, 1),
            new Rectangle(25, 1, 1, 1),
            new Rectangle(32, 8, 1, 1),
            new Rectangle(34, 11, 1, 1),
            new Rectangle(13, 12, 1, 1),
            new Rectangle(37, 16, 1, 1),
            new Rectangle(9, 19, 1, 1),
            new Rectangle(6, 21, 1, 1),
            new Rectangle(43, 23, 1, 1),
            new Rectangle(3, 24, 1, 1),
            new Rectangle(1, 26, 1, 1),
            new Rectangle(46, 26, 1, 1)
        ];

    public CollisionType CollisionType => CollisionType.Player;

    public bool IsHot => true;
    
    public bool ShouldRemoveOnCollision => !IsAlive; // Only remove when player dies

    private void DrawCollisionBox(SpriteBatch spriteBatch)
    {
        if (Global.DebugShowCollisionBoxes)
        {
            Texture2D texture = spriteBatch.BlankTexture();
            foreach (var r in CollisionRectangles)
            {
                spriteBatch.Draw(texture, r, Color.Yellow);
            }            
        }
    }

    public void OnCollide(ICollision other)
    {
        // Get damage from projectile if it's a projectile collision       
        if (other is Projectile projectile)
        {
            int damage = 0; // Default damage
            damage = (int)projectile.Damage;
            TakeDamage(damage);
            System.Diagnostics.Debug.WriteLine($"Player took {damage} damage from {other.CollisionType}. Health: {Health}/{MaxHealth}");

        }

        // Only explode and destroy when health reaches zero
        if (!IsAlive)
        {
            DestroyPlayer();
        }
    }

    public void TakeDamage(int damage)
    {
        Health = Math.Max(0, Health - damage);
    }

    private void DestroyPlayer()
    {
        // Create explosion at player center
        var explosionInfo = new ExplosionInfo(Center, 48);
        Global.World.ExplosionManager.Register(explosionInfo);

        // Create smoke area centered around the player
        var smokeRadius = Size96.HalfWidth.X * 1.5f;
        var smokeParticleCount = (int)(30 * 1.5f); // Scale particles with radius
        var smokeInfo = new SmokeAreaInfo(Center, smokeRadius, smokeParticleCount, 0.1f);
        Global.World.SmokeManager.Register(smokeInfo);

        // Unregister from collision system to prevent further collisions
        Global.World.CollisionManager.UnRegister(this);

        // Make player disappear (invisible)
        // TODO: Add game over logic, respawn logic, or other destruction handling here
        System.Diagnostics.Debug.WriteLine($"Player destroyed at {Center}");
    }

    #endregion


    public Vector2 MapPosition          { get; set; } = Global.World.GlobalPlayerPosition;
    public Vector2 Center               => MapPosition - Size48.Center; // Center of collision box
    public float MovementSpeed          { get; set; } = 0.35f;
    public int Health                   { get; private set; } = 5; // Player starts with 5 hitpoints
    public int MaxHealth                { get; private set; } = 5;
    public bool IsAlive                 => Health > 0;

    public Texture2D Texture            { get; private set; }
    public Dictionary<int, SpriteFrame> Frames 
                                        { get; private set; } = new Dictionary<int, SpriteFrame>();

    // Weapons
    public IWeapon MainWeapon           { get; set; }

}
