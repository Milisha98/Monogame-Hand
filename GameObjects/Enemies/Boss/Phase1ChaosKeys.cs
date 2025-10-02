using System;
using System.Collections.Generic;
using System.Linq;
using Hands.Core.Animation;

namespace Hands.GameObjects.Enemies.Boss;

public class Phase1ChaosKeys : IBossPhase
{
    private readonly TimeSpan _phaseDuration = TimeSpan.FromSeconds(90); // 90 seconds (1.5 minutes) for Phase 1
    private readonly TimeSpan _keyGlowInterval = TimeSpan.FromSeconds(2); // New key glows every 2 seconds
    
    private Tween _phaseTween;
    private Tween _keyGlowTween;
    private readonly Random _random = new();
    private Key _currentGlowingKey = null;

    public bool IsComplete => _phaseTween?.IsComplete ?? false;
    public BossPhase? NextPhase => BossPhase.Phase2_SpellShutdown;

    public void OnEnter(Boss boss, IEnumerable<Key> keys)
    {
        _phaseTween = new Tween(_phaseDuration);
        _keyGlowTween = new Tween(_keyGlowInterval);
        _keyGlowTween.OnCompleted += () => OnKeyGlowIntervalCompleted(keys.ToList());
        _currentGlowingKey = null;
        
        // Start with the first random key immediately
        SelectRandomKeyToGlow(keys.ToList());
    }

    public void Update(GameTime gameTime, Boss boss, IEnumerable<Key> keys)
    {
        // Update phase timer
        _phaseTween?.Update(gameTime);
        
        // Update key glow interval timer
        _keyGlowTween?.Update(gameTime);
    }

    public void OnExit(Boss boss, IEnumerable<Key> keys)
    {
        // Stop all glowing when exiting phase 1
        foreach (var key in keys)
        {
            key.StopGlow();
        }
        _currentGlowingKey = null;
        
        // Clean up tweens
        _phaseTween = null;
        _keyGlowTween = null;
    }

    private void OnKeyGlowIntervalCompleted(List<Key> keys)
    {
        // Select a new random key to glow
        SelectRandomKeyToGlow(keys);
        
        // Reset the interval timer for the next key
        _keyGlowTween.Reset();
    }

    private void SelectRandomKeyToGlow(List<Key> keys)
    {
        if (keys.Count == 0) return;
        
        // Stop current glowing key if any
        _currentGlowingKey?.StopGlow();
        
        // Select a random key
        int randomIndex = _random.Next(keys.Count);
        _currentGlowingKey = keys[randomIndex];
        
        // Start glowing with a 1.5 second pulse duration, no repeat
        // (it will be replaced by the next random key)
        var glowSettings = new GlowSettings(true, false, 1.5);
        _currentGlowingKey.StartGlow(glowSettings);
    }
}