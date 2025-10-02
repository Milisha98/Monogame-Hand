using System;
using System.Collections.Generic;
using System.Linq;
using Hands.Core;
using Hands.Core.Animation;

namespace Hands.GameObjects.Enemies.Boss;

public class Phase1ChaosKeys : IBossPhase
{
    private readonly TimeSpan _phaseDuration = TimeSpan.FromSeconds(90); // 90 seconds (1.5 minutes) for Phase 1
    private readonly TimeSpan _keySelectionInterval = TimeSpan.FromSeconds(5); // Select new key every 5 seconds
    
    // Single glow workflow managed by the phase
    private readonly WorkflowStage<KeyGlowState>[] _glowWorkflowStages = 
    [
        new(KeyGlowState.GlowUp, TimeSpan.FromSeconds(2.0)), // Glow up to full intensity
        new(KeyGlowState.Shoot, TimeSpan.FromMilliseconds(1)), // Instant shoot
        new(KeyGlowState.HoldGlow, TimeSpan.FromSeconds(2.0)) // Hold glow for 2 seconds
    ];
    
    private Tween _phaseTween;
    private Tween _keySelectionTween;
    private Workflow<KeyGlowState> _glowWorkflow;
    private readonly Random _random = new();
    private Key _currentGlowingKey = null;
    private bool _keyWasInterrupted = false;

    public bool IsComplete => _phaseTween?.IsComplete ?? false;
    public BossPhase? NextPhase => BossPhase.Phase2_SpellShutdown;

    public void OnEnter(Boss boss, IEnumerable<Key> keys)
    {
        _phaseTween = new Tween(_phaseDuration);
        _keySelectionTween = new Tween(_keySelectionInterval);
        _keySelectionTween.OnCompleted += () => OnKeySelectionCompleted(keys.ToList());
        
        // Initialize the glow workflow
        _glowWorkflow = new Workflow<KeyGlowState>(_glowWorkflowStages);
        _glowWorkflow.OnStateChanged += OnGlowStateChanged;
        _glowWorkflow.OnCompleted += OnGlowCycleCompleted;
        
        _currentGlowingKey = null;
        
        // Start with the first random key immediately
        SelectRandomKeyToStartGlowing(keys.ToList());
    }

    public void Update(GameTime gameTime, Boss boss, IEnumerable<Key> keys)
    {
        // Update phase timer
        var phasePercent = _phaseTween?.Update(gameTime) ?? 0f;
        
        // Update key selection timer
        var keySelectionPercent = _keySelectionTween?.Update(gameTime) ?? 0f;
        
        // Update glow workflow
        _glowWorkflow?.Update(gameTime);
        
        // Update current key's glow intensity based on workflow state
        UpdateCurrentKeyGlow();
    }

    public void OnExit(Boss boss, IEnumerable<Key> keys)
    {
        // Stop all glowing when exiting phase 1
        foreach (var key in keys)
        {
            key.GlowIntensity = 0f;
            // Unregister all keys from collision detection
            Global.World.CollisionManager.UnRegister(key);
        }
        _currentGlowingKey = null;
        
        // Clean up tweens and workflow
        _phaseTween = null;
        _keySelectionTween = null;
        _glowWorkflow = null;
    }

    private void OnKeySelectionCompleted(List<Key> keys)
    {
        // Select a new random key to glow
        SelectRandomKeyToStartGlowing(keys);
        
        // Reset the interval timer for the next key
        _keySelectionTween.Reset();
    }

    private void SelectRandomKeyToStartGlowing(List<Key> keys)
    {
        if (keys.Count == 0)
            return;
        
        // Clear current glowing key if any
        if (_currentGlowingKey != null)
        {
            _currentGlowingKey.GlowIntensity = 0f;
            // Unregister previous key from collision detection
            Global.World.CollisionManager.UnRegister(_currentGlowingKey);
        }
        
        // Select a random key
        int randomIndex = _random.Next(keys.Count);
        _currentGlowingKey = keys[randomIndex];
        
        // Set up interruption callback
        _currentGlowingKey.OnInterrupted = () => OnCurrentKeyInterrupted();
        
        // Register the key for collision detection during glow phase
        Global.World.CollisionManager.Register(_currentGlowingKey);
        
        // Reset the glow workflow to start from the beginning
        _glowWorkflow.Reset();
        _glowWorkflow.IsActive = true;
        _keyWasInterrupted = false;
    }

    private void UpdateCurrentKeyGlow()
    {
        if (_currentGlowingKey == null || _glowWorkflow == null || _keyWasInterrupted)
            return;

        // Calculate glow intensity based on current workflow state
        float intensity = 0f;
        if (_glowWorkflow.CurrentState == KeyGlowState.GlowUp)
        {
            intensity = EaseInOutSine(_glowWorkflow.CurrentPercent);
        }
        else if (_glowWorkflow.CurrentState == KeyGlowState.Shoot || _glowWorkflow.CurrentState == KeyGlowState.HoldGlow)
        {
            intensity = 1f; // Full glow during shoot and hold phases
        }

        _currentGlowingKey.GlowIntensity = intensity;
    }

    private void OnGlowStateChanged(KeyGlowState state)
    {
        if (state == KeyGlowState.Shoot && _currentGlowingKey != null)
        {
            _currentGlowingKey.ShootBulletFan();
        }
    }

    private void OnGlowCycleCompleted()
    {
        // In Phase 1, keys don't repeat - they glow once and then wait for next selection
        if (_currentGlowingKey != null)
        {
            _currentGlowingKey.GlowIntensity = 0f;
            // Unregister key from collision detection
            Global.World.CollisionManager.UnRegister(_currentGlowingKey);
        }
    }

    private void OnCurrentKeyInterrupted()
    {
        _keyWasInterrupted = true;
        
        // Stop the workflow to prevent further updates
        if (_glowWorkflow != null)
        {
            _glowWorkflow.IsActive = false;
        }
        
        // Unregister key from collision detection since it's no longer glowing
        if (_currentGlowingKey != null)
        {
            Global.World.CollisionManager.UnRegister(_currentGlowingKey);
        }
        
        // Key intensity is already set to 0 by the Key.OnCollide method
        // We just need to prevent the phase from overriding it
    }

    /// <summary>
    /// EaseInOutSine easing function from https://easings.net/#easeInOutSine
    /// </summary>
    private static float EaseInOutSine(float x)
    {
        return -(MathF.Cos(MathF.PI * x) - 1) / 2;
    }
}