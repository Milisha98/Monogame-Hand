using Hands.GameObjects.Enemies.Turret;
using System.Collections.Generic;

namespace Hands.Core.Managers;
internal class SleepManager : IUpdate
{
    private List<ISleep> _register { get; set; } = [];

    public void Register(ISleep target)
    {
        if (_register.Contains(target)) return;
        _register.Add(target);
    }

    public void Unregister(ISleep target)
    {
        if (_register.Contains(target) == false) return;
        _register.Remove(target);
    }

    public void Update(GameTime gameTime)
    {
        foreach (var target in _register)
        {
            float distanceToPlayer = Math.Abs(target.MapPosition.Y - Global.World.Camera.Center.Y);
            if (distanceToPlayer > target.WakeDistance)
            {
                if (!target.IsAsleep)
                    target.OnSleep();
            }
            else // distanceToPlayer <= target.TriggerDistance
            {
                if (target.IsAsleep)
                    target.OnSisterAwake();
            }
        }
    }

}