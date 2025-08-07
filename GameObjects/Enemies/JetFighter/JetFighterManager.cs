using Hands.Core;
using Hands.Core.Sprites;
using Hands.Sprites;
using Microsoft.Xna.Framework.Content;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hands.GameObjects.Enemies.JetFighter;

internal class JetFighterManager : ILoadContent, IUpdate, IDraw
{
    private ConcurrentDictionary<JetFighter, byte> JetFighters { get; set; } = new();

    public void LoadContent(ContentManager contentManager)
    {
        Sprite.LoadContent(contentManager);
    }

    public void Register(JetFighterInfo info)
    {
        var jetFighter = new JetFighter(info);
        JetFighters.TryAdd(jetFighter, 0);
        Global.World.SleepManager.Register(jetFighter);
    }

    public void Unregister(JetFighter jetFighter)
    {
        JetFighters.TryRemove(jetFighter, out _);
        Global.World.SleepManager.Unregister(jetFighter);
    }

    public void Update(GameTime gameTime)
    {
        Parallel.ForEach(JetFighters.Keys, jetFighter =>
        {
            jetFighter.Update(gameTime);
        });
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Parallel.ForEach(JetFighters.Keys, jetFighter =>
        {
            jetFighter.Draw(spriteBatch);
        });
    }

    //
    // Properties
    //
    public JetFighterSprite Sprite { get; init; } = new();
}

internal class JetFighterSprite
{
    private const string AssetName = "JetFighter";

    public void LoadContent(ContentManager contentManager)
    {
        Texture = contentManager.Load<Texture2D>(AssetName);
        Frames = SpriteHelper.CreateFramesFromTexture(Texture, Size30.Point);
    }

    public Texture2D Texture { get; private set; }
    public Dictionary<int, SpriteFrame> Frames { get; private set; }
}

internal record JetFighterInfo(string ID, int X, int Y, float MovementSpeed, float WakeDistance);
