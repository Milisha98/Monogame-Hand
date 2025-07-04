﻿using Hands.GameObjects.Enemies.Turret;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        Parallel.ForEach(_register, target =>
        {
            float distanceToPlayer = Math.Abs(target.MapPosition.Y - Global.World.Player.MapPosition.Y);
            //System.Diagnostics.Debug.WriteLine($"Distance to Player: {distanceToPlayer} - Target: {target.GetType().Name}");
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
        });
    }

}