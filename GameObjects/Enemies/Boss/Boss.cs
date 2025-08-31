using Hands.Core;
using Hands.Core.Managers.Collision;
using Hands.Core.Sprites;
using Hands.Sprites;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.GameObjects.Enemies.Boss;
public class Boss : ILoadContent, IMapPosition, ISleep, IUpdate, IDraw
{
    const float Width = 896;
    const float Height = 320;

    private readonly BossInfo _info;
    private readonly Key _keyboard;

    public Boss(BossInfo info)
    {
        MapPosition = new Vector2(info.X, info.Y);
        _info = info;
        WakeDistance = _info.WakeDistance <= 0f ? Global.World.GlobalWakeDistance : _info.WakeDistance;

        _keyboard = new Key(new KeyInfo(info.X, info.Y, 896, 300));

        // Register with SleepManager
        Global.World.SleepManager.Register(this);
    }

    #region ILoadContent

    public void LoadContent(ContentManager contentManager)
    {
        Sprite.LoadContent(contentManager);
    }

    #endregion

    #region IMapPosition

    public Vector2 MapPosition { get; set; }
    public Vector2 Center => MapPosition + (Dimensions / 2);
    public Vector2 Dimensions { get => new Vector2(Width, Height); }

    #endregion

    #region IUpdate

    public void Update(GameTime gameTime)
    {
        if (State == BossState.Destroyed) return;
        if (State == BossState.Asleep) return;

        // Boss update logic will go here when active
        // For now, just maintain active state
    }

    #endregion

    #region IDraw

    public void Draw(SpriteBatch spriteBatch)
    {
        // First draw they Keyboard
        _keyboard.Draw(spriteBatch);

        // Draw the Keys

        // Draw the Keys Shadows

        // Draw the Shadows
        _keyboard.DrawShadow(spriteBatch);

    }

    #endregion

    #region ISleep

    public float WakeDistance { get; init; }
    public bool IsAsleep { get; private set; } = true;

    public void OnSisterAwake()
    {
        if (State == BossState.Destroyed) return;
        State = BossState.Active;
        IsAsleep = false;

        // Boss-specific wake up logic can be added here
    }

    public void OnSleep()
    {
        // Boss should NOT fall back asleep once awakened
        // This method intentionally does nothing
    }

    #endregion

    #region Properties

    //
    // Properties
    //
    public KeyboardSprite Sprite { get; init; } = new();
    public BossState State { get; private set; } = BossState.Asleep;

    #endregion
   

}

public class KeyboardSprite
{
    private const string AssetNameKeys = "Keys-Sheet";

    public void LoadContent(ContentManager contentManager)
    {
        Texture = contentManager.Load<Texture2D>(AssetNameKeys);
        Frames = SpriteHelper.CreateFramesFromTexture(Texture, Size12.Point);
    }

    public Texture2D Texture { get; private set; }
    public Dictionary<int, SpriteFrame> Frames { get; private set; }
}

