using Hands.Core;
using Hands.Core.Sprites;
using Hands.Sprites;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hands.GameObjects.Enemies.Mobile;
internal class MobileManager : ILoadContent, IUpdate, IDraw
{
    private List<Mobile> Mobiles { get; set; } = new();

    public void LoadContent(ContentManager contentManager)
    {
        Sprite.LoadContent(contentManager);
    }

    public void Register(MobileInfo info)
    {
        var mobile = new Mobile(info);
        Mobiles.Add(mobile);
        Global.World.SleepManager.Register(mobile);
    }

    public void Unregister(Mobile mobile)
    {
        Mobiles.Remove(mobile);
        Global.World.SleepManager.Unregister(mobile);
    }

    public void Update(GameTime gameTime)
    {
        Parallel.ForEach(Mobiles, mobile =>
        {
            mobile.Update(gameTime);
        });
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Parallel.ForEach(Mobiles, mobile =>
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
