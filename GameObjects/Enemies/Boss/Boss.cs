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
    private readonly HashSet<Key> _keys;

    public Boss(BossInfo info)
    {
        MapPosition = new Vector2(info.X, info.Y);
        _info = info;
        WakeDistance = _info.WakeDistance <= 0f ? Global.World.GlobalWakeDistance : _info.WakeDistance;

        _keyboard = new Key(new KeyInfo(info.X, info.Y, 896, 300));
        _keys = InitiateKeys();

        // Register with SleepManager
        Global.World.SleepManager.Register(this);
    }

    private HashSet<Key> InitiateKeys()
    { 
        KeyInfo[] keys = 
            [   new KeyInfo(4, 4, 48, 48, "~", "`"),
                new KeyInfo(64, 4, 48, 48, "!", "1"),
                new KeyInfo(124, 4, 48, 48, "@", "2"),
                new KeyInfo(184, 4, 48, 48, "#", "3"),
                new KeyInfo(244, 4, 48, 48, "$", "4"),
                new KeyInfo(304, 4, 48, 48, "%", "5"),
                new KeyInfo(364, 4, 48, 48, "^", "6"),
                new KeyInfo(424, 4, 48, 48, "&", "7"),
                new KeyInfo(484, 4, 48, 48, "*", "8"),
                new KeyInfo(544, 4, 48, 48, "(", "9"),
                new KeyInfo(604, 4, 48, 48, ")", "0"),
                new KeyInfo(664, 4, 48, 48, "_", "-"),
                new KeyInfo(724, 4, 48, 48, "+", "="),
                new KeyInfo(784, 4, 108, 48, "Backspace"),

                // Second Row
                new KeyInfo(4, 64, 84, 48, "Tab"),
                new KeyInfo(104, 64, 48, 48, "Q"),
                new KeyInfo(164, 64, 48, 48, "W"),
                new KeyInfo(224, 64, 48, 48, "E"),
                new KeyInfo(284, 64, 48, 48, "R"),
                new KeyInfo(344, 64, 48, 48, "T"),
                new KeyInfo(404, 64, 48, 48, "Y"),
                new KeyInfo(464, 64, 48, 48, "U"),
                new KeyInfo(524, 64, 48, 48, "I"),
                new KeyInfo(584, 64, 48, 48, "O"),
                new KeyInfo(644, 64, 48, 48, "P"),
                new KeyInfo(704, 64, 48, 48, "{", "["),
                new KeyInfo(764, 64, 48, 48, "}", "]"),
                new KeyInfo(824, 64, 68, 48, "|", "\\"),

                // Third Row
                new KeyInfo(4, 124, 96, 48, "CapsLock"),
                new KeyInfo(124, 124, 48, 48, "A"),
                new KeyInfo(184, 124, 48, 48, "S"),
                new KeyInfo(244, 124, 48, 48, "D"),
                new KeyInfo(304, 124, 48, 48, "F"),
                new KeyInfo(364, 124, 48, 48, "G"),
                new KeyInfo(424, 124, 48, 48, "H"),
                new KeyInfo(484, 124, 48, 48, "J"),
                new KeyInfo(544, 124, 48, 48, "K"),
                new KeyInfo(604, 124, 48, 48, "L"),
                new KeyInfo(664, 124, 48, 48, ":", ";"),
                new KeyInfo(724, 124, 48, 48, "\"", "'"),
                new KeyInfo(784, 124, 108, 48, "Return"),

                // Fourth Row
                new KeyInfo(4, 184, 120, 48, "Shift"),
                new KeyInfo(144, 184, 48, 48, "Z"),
                new KeyInfo(204, 184, 48, 48, "X"),
                new KeyInfo(264, 184, 48, 48, "C"),
                new KeyInfo(324, 184, 48, 48, "V"),
                new KeyInfo(384, 184, 48, 48, "B"),
                new KeyInfo(444, 184, 48, 48, "N"),
                new KeyInfo(504, 184, 48, 48, "M"),
                new KeyInfo(564, 184, 48, 48, "<", ","),
                new KeyInfo(624, 184, 48, 48, ">", "."),
                new KeyInfo(684, 184, 48, 48, "?", "/"),
                new KeyInfo(744, 184, 148, 48, "Shift"),

                // Fifth Row
                new KeyInfo(4, 244, 60, 48, "Ctrl"),
                new KeyInfo(84, 244, 60, 48, "Win"),
                new KeyInfo(164, 244, 60, 48, "Alt"),
                new KeyInfo(244, 244, 336, 48, "Space"),
                new KeyInfo(592, 244, 60, 48, "Alt"),
                new KeyInfo(672, 244, 60, 48, "Win"),
                new KeyInfo(752, 244, 60, 48, "Menu"),
                new KeyInfo(832, 244, 60, 48, "Ctrl")

            ];

        return keys.Select(k => new KeyInfo(_info.X + k.X, _info.Y + k.Y, k.Width, k.Height, k.Key1, k.Key2))
                   .Select(k => new Key(k))
                   .ToHashSet();
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
        foreach (var key in _keys)
        {
            key.Draw(spriteBatch);
        }

        // Draw the Keys Shadows
        foreach (var key in _keys)
        {
            key.DrawShadow(spriteBatch);
        }

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

