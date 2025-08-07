using Hands.Core;
using Hands.Core.Sprites;
using Hands.Sprites;
using Microsoft.Xna.Framework.Content;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hands.GameObjects.Enemies.Mobile;

internal class MobileManager : ILoadContent, IUpdate, IDraw
{
    private ConcurrentDictionary<Mobile, byte> Mobiles { get; set; } = new();

    public void LoadContent(ContentManager contentManager)
    {
        Sprite.LoadContent(contentManager);
    }

    public void Register(MobileInfo info)
    {
        var mobile = new Mobile(info);
        Mobiles.TryAdd(mobile, 0);
        Global.World.SleepManager.Register(mobile);
    }

    public void Unregister(Mobile mobile)
    {
        Mobiles.TryRemove(mobile, out _);
        Global.World.SleepManager.Unregister(mobile);
    }

    public void Update(GameTime gameTime)
    {
        Parallel.ForEach(Mobiles.Keys, mobile =>
        {
            mobile.Update(gameTime);
        });
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Parallel.ForEach(Mobiles.Keys, mobile =>
        {
            mobile.Draw(spriteBatch);
        });
    }

    //
    // Properties
    //
    public MobileSprite Sprite { get; init; } = new();
}

internal class MobileSprite
{
    private const string AssetName = "Mobile";

    public void LoadContent(ContentManager contentManager)
    {
        Texture = contentManager.Load<Texture2D>(AssetName);
        Frames = SpriteHelper.CreateFramesFromTexture(Texture, Size40.Point);
    }

    public Texture2D Texture { get; private set; }
    public Dictionary<int, SpriteFrame> Frames { get; private set; }
}
