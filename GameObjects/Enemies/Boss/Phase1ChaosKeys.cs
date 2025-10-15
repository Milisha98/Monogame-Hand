using System;
using System.Collections.Generic;
using System.Linq;
using Hands.Core;
using Hands.Core.Animation;

namespace Hands.GameObjects.Enemies.Boss;

public enum PhaseEscalationLevel
{
    Level1,     // 0-30 seconds: 2 keys
    Level2,     // 30-60 seconds: 3 keys
    Level3      // 60-90 seconds: 4 keys
}

public class Phase1ChaosKeys : IBossPhase
{
    private readonly TimeSpan _phaseDuration = TimeSpan.FromSeconds(90); // 90 seconds (1.5 minutes) for Phase 1
    private readonly TimeSpan _keySelectionInterval = TimeSpan.FromSeconds(5); // Select new key every 5 seconds
    
    // Phase escalation workflow stages
    private readonly WorkflowStage<PhaseEscalationLevel>[] _escalationStages = 
    [
        new(PhaseEscalationLevel.Level1, TimeSpan.FromSeconds(30)),    // 0-30s: 2 keys
        new(PhaseEscalationLevel.Level2, TimeSpan.FromSeconds(30)),  // 30-60s: 3 keys
        new(PhaseEscalationLevel.Level3, TimeSpan.FromSeconds(30))    // 60-90s: 4 keys
    ];
    
    // Glow workflow template for individual keys
    private readonly WorkflowStage<KeyGlowState>[] _glowWorkflowStages = 
    [
        new(KeyGlowState.GlowUp, TimeSpan.FromSeconds(2.0)), // Glow up to full intensity
        new(KeyGlowState.Shoot, TimeSpan.FromMilliseconds(1)), // Instant shoot
        new(KeyGlowState.HoldGlow, TimeSpan.FromSeconds(2.0)) // Hold glow for 2 seconds
    ];
    
    private Tween _phaseTween;
    private Tween _keySelectionTween;
    private Workflow<PhaseEscalationLevel> _escalationWorkflow;
    private readonly Random _random = new();
    
    // Active glowing keys and their workflows
    private readonly List<GlowingKeyData> _activeGlowingKeys = [];
    
    // Boss movement
    private int _movementDirection = 1; // 1 for right, -1 for left
    private const float MovementSpeed = 0.5f; // pixels per frame

    public bool IsComplete => _phaseTween?.IsComplete ?? false;
    public BossPhase? NextPhase => BossPhase.Phase2_SpellShutdown;

    public void OnEnter(Boss boss, IEnumerable<Key> keys)
    {
        _phaseTween = new Tween(_phaseDuration);
        _keySelectionTween = new Tween(_keySelectionInterval);
        _keySelectionTween.OnCompleted += () => OnKeySelectionCompleted(keys.ToList());
        
        // Initialize the escalation workflow
        _escalationWorkflow = new Workflow<PhaseEscalationLevel>(_escalationStages);
        _escalationWorkflow.OnStateChanged += OnEscalationLevelChanged;
        
        // Clear any existing glowing keys
        _activeGlowingKeys.Clear();
        
        // Start with the first set of keys based on initial escalation level (Level1_OneKey)
        SelectKeysForCurrentLevel(keys.ToList());
    }

    public void Update(GameTime gameTime, Boss boss, IEnumerable<Key> keys)
    {
        // Update phase timer
        var phasePercent = _phaseTween?.Update(gameTime) ?? 0f;
        
        // Update escalation workflow to handle difficulty progression
        _escalationWorkflow?.Update(gameTime);
        
        // Update key selection timer
        var keySelectionPercent = _keySelectionTween?.Update(gameTime) ?? 0f;
        
        // Update all active glowing keys and their individual workflows
        UpdateActiveGlowingKeys(gameTime);
        
        // Update boss horizontal movement
        UpdateBossMovement(boss);
    }

    public void OnExit(Boss boss, IEnumerable<Key> keys)
    {
        // Stop all glowing when exiting phase 1
        foreach (var glowingKeyData in _activeGlowingKeys)
        {
            glowingKeyData.Key.GlowIntensity = 0f;
            // Unregister all active keys from collision detection
            Global.World.CollisionManager.UnRegister(glowingKeyData.Key);
        }
        
        // Clear active glowing keys list
        _activeGlowingKeys.Clear();
        
        // Clean up tweens and workflows
        _phaseTween = null;
        _keySelectionTween = null;
        _escalationWorkflow = null;
    }

    private void OnKeySelectionCompleted(List<Key> keys)
    {
        // Select new keys based on current escalation level
        SelectKeysForCurrentLevel(keys);
        
        // Reset the interval timer for the next selection
        _keySelectionTween.Reset();
    }

    private void SelectKeysForCurrentLevel(List<Key> keys)
    {
        if (keys.Count == 0)
            return;
        
        // Clear all current glowing keys
        ClearAllActiveGlowingKeys();
        
        // Determine how many keys to select based on current escalation level
        int keyCount = GetKeyCountForCurrentLevel();
        
        // Select random keys (ensuring no duplicates)
        var selectedKeys = keys.OrderBy(x => _random.Next()).Take(keyCount).ToList();
        
        // Create glowing key data for each selected key
        foreach (var key in selectedKeys)
        {
            StartKeyGlowing(key);
        }
    }
    
    private void StartKeyGlowing(Key key)
    {
        // Create a new workflow for this key
        var workflow = new Workflow<KeyGlowState>(_glowWorkflowStages);
        workflow.OnStateChanged += (state) => OnKeyGlowStateChanged(key, state);
        workflow.OnCompleted += () => OnKeyGlowCompleted(key);
        
        // Create glowing key data
        var glowingKeyData = new GlowingKeyData(key, workflow);
        _activeGlowingKeys.Add(glowingKeyData);
        
        // Set up interruption callback
        key.OnInterrupted = () => OnKeyInterrupted(key);
        
        // Register the key for collision detection during glow phase
        Global.World.CollisionManager.Register(key);
        
        // Start the workflow
        workflow.Reset();
        workflow.IsActive = true;
    }
    
    private int GetKeyCountForCurrentLevel()
    {
        return _escalationWorkflow?.CurrentState switch
        {
            PhaseEscalationLevel.Level1 => 3,
            PhaseEscalationLevel.Level2 => 4,
            PhaseEscalationLevel.Level3 => 6,
            _ => 2 // Default fallback
        };
    }
    
    private void ClearAllActiveGlowingKeys()
    {
        foreach (var glowingKeyData in _activeGlowingKeys)
        {
            glowingKeyData.Key.GlowIntensity = 0f;
            Global.World.CollisionManager.UnRegister(glowingKeyData.Key);
        }
        _activeGlowingKeys.Clear();
    }

    private void UpdateActiveGlowingKeys(GameTime gameTime)
    {
        // Update each active glowing key's workflow and intensity
        foreach (var glowingKeyData in _activeGlowingKeys.ToList()) // ToList() to avoid modification during iteration
        {
            if (glowingKeyData.WasInterrupted)
                continue;
            
            // Update the key's individual workflow
            glowingKeyData.Workflow.Update(gameTime);
            
            // IMPORTANT: Only update intensity if workflow is still active and not completed
            // This prevents overriding the intensity after OnKeyGlowCompleted sets it to 0
            if (!glowingKeyData.Workflow.IsComplete && glowingKeyData.Workflow.IsActive)
            {
                // Calculate glow intensity based on current workflow state
                float intensity = CalculateGlowIntensity(glowingKeyData.Workflow);
                glowingKeyData.Key.GlowIntensity = intensity;
            }
        }
    }
    
    private float CalculateGlowIntensity(Workflow<KeyGlowState> workflow)
    {
        return workflow.CurrentState switch
        {
            KeyGlowState.GlowUp => EaseInOutSine(workflow.CurrentPercent),
            KeyGlowState.Shoot or KeyGlowState.HoldGlow => 1f,
            _ => 0f
        };
    }

    private void OnEscalationLevelChanged(PhaseEscalationLevel newLevel)
    {
        // When escalation level changes, we don't immediately change the keys
        // We wait for the next key selection interval to apply the new difficulty
        // This provides a smooth transition between difficulty levels
    }
    
    private void OnKeyGlowStateChanged(Key key, KeyGlowState state)
    {
        if (state == KeyGlowState.Shoot)
        {
            key.ShootBulletFan();
        }
    }

    private void OnKeyGlowCompleted(Key key)
    {
        // Key glow cycle completed - set intensity to 0 and unregister from collision
        key.GlowIntensity = 0f;
        Global.World.CollisionManager.UnRegister(key);
        
        // Remove the completed key from the active glowing keys list
        var completedKeyData = _activeGlowingKeys.FirstOrDefault(gkd => gkd.Key == key);
        if (completedKeyData != null)
        {
            _activeGlowingKeys.Remove(completedKeyData);
        }
    }

    private void OnKeyInterrupted(Key interruptedKey)
    {
        // Find the glowing key data for the interrupted key
        var glowingKeyData = _activeGlowingKeys.FirstOrDefault(gkd => gkd.Key == interruptedKey);
        if (glowingKeyData != null)
        {
            glowingKeyData.WasInterrupted = true;
            
            // Stop the workflow to prevent further updates
            glowingKeyData.Workflow.IsActive = false;
            
            // Unregister key from collision detection since it's no longer glowing
            Global.World.CollisionManager.UnRegister(interruptedKey);
            
            // Key intensity is already set to 0 by the Key.OnCollide method
            // We just need to prevent the phase from overriding it
        }
    }

    /// <summary>
    /// EaseInOutSine easing function from https://easings.net/#easeInOutSine
    /// </summary>
    private static float EaseInOutSine(float x)
    {
        return -(MathF.Cos(MathF.PI * x) - 1) / 2;
    }
    
    /// <summary>
    /// Updates boss horizontal movement between x=64 and x=1088-896=192
    /// </summary>
    private void UpdateBossMovement(Boss boss)
    {
        const float LeftBound = 64f;
        const float RightBound = 1088f - 896f; // Screen right - boss width
        
        Vector2 movementDelta = new Vector2(MovementSpeed * _movementDirection, 0);
        
        // Check bounds and reverse direction if needed
        float newX = boss.MapPosition.X + movementDelta.X;
        if (newX <= LeftBound)
        {
            _movementDirection = 1; // Move right
            movementDelta.X = LeftBound - boss.MapPosition.X; // Clamp to bound
        }
        else if (newX >= RightBound)
        {
            _movementDirection = -1; // Move left  
            movementDelta.X = RightBound - boss.MapPosition.X; // Clamp to bound
        }
        
        // Apply movement to boss and all keys
        boss.MapPosition += movementDelta;
        
        // Move all keys with the boss
        foreach (var key in boss.Keys)
        {
            key.MapPosition += movementDelta;
        }
        
        // Also move the keyboard frame
        boss.KeyboardFrame.MapPosition += movementDelta;
    }
}
