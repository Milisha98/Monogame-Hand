using Hands.Core;
using Hands.Core.Sprites;
using Hands.Sprites;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hands.GameObjects.Enemies.SideGun;

internal class SideGunManager : ILoadContent, IUpdate, IDraw
{
    private List<SideGun> SideGuns { get; set; } = new();
    
    public void LoadContent(ContentManager contentManager)
    {
        Sprite.LoadContent(contentManager);
    }

    public void Register(SideGunInfo info)
    {
        var sideGun = new SideGun(info);
        SideGuns.Add(sideGun);
        Global.World.SleepManager.Register(sideGun);
    }

    public void Unregister(SideGun sideGun)
    {
        SideGuns.Remove(sideGun);
        Global.World.SleepManager.Unregister(sideGun);
    }

    public void Update(GameTime gameTime)
    {
        Parallel.ForEach(SideGuns, sideGun =>
        {
            sideGun.Update(gameTime);
        });
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Parallel.ForEach(SideGuns, sideGun =>
        {
            sideGun.Draw(spriteBatch);
        });
    }

    public SideGunSprite Sprite { get; init; } = new();
}

internal class SideGunSprite
{
    private const string AssetName = "SideGun";

    public void LoadContent(ContentManager contentManager)
    {
        Texture = contentManager.Load<Texture2D>(AssetName);
        Frames = SpriteHelper.CreateFramesFromTexture(Texture, Size32.Point);
    }

    public Texture2D Texture { get; private set; }
    public Dictionary<int, SpriteFrame> Frames { get; private set; }
}

internal record SideGunInfo(string ID, int X, int Y, SideGunOrientation Orientation, float WakeDistance);
