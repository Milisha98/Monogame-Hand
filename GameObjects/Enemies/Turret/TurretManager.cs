using Hands.Core;
using Hands.Core.Sprites;
using Hands.Sprites;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hands.GameObjects.Enemies.Turret;
internal class TurretManager : ILoadContent, IUpdate, IDraw
{
    private List<Turret> Turrets { get; set; } = new();
    public void LoadContent(ContentManager contentManager)
    {
        Sprite.LoadContent(contentManager);
    }

    public void Register(TurretInfo info)
    {
        var turret = new Turret(info);
        Turrets.Add(turret);
        Global.World.SleepManager.Register(turret);
    }

    public void Unregister(Turret turret)
    {
        Turrets.Remove(turret);
        Global.World.SleepManager.Unregister(turret);
    }

    public void Update(GameTime gameTime)
    {
        Parallel.ForEach(Turrets, turret =>
        {
            turret.Update(gameTime);
        });
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Parallel.ForEach(Turrets, turret =>
        {
            turret.Draw(spriteBatch);
        });
    }

    //
    // Properties
    //
    public TurretSprite Sprite { get; init; } = new();
}


internal class TurretSprite
{
    private const string AssetName = "Tilesets/Turret";

    public void LoadContent(ContentManager contentManager)
    {
        Texture = contentManager.Load<Texture2D>(AssetName);
        Frames = SpriteHelper.CreateFramesFromTexture(Texture, Size64.Point);
    }

    public Texture2D Texture { get; private set; }
    public Dictionary<int, SpriteFrame> Frames { get; private set; }
}

internal record TurretInfo(string ID, int X, int Y, TurretStyle Style, float RoF, float WakeDistance);