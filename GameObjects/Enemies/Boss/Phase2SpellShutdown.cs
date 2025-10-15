using System;
using System.Collections.Generic;
using System.Linq;
using Hands.Core;
using Hands.GameObjects.Enemies.JetFighter;

namespace Hands.GameObjects.Enemies.Boss;

public class Phase2SpellShutdown : IBossPhase
{
    private const string SHUTDOWN_SEQUENCE = "SHUTDOWN";
    private const float SHOOT_INTERVAL = 5f; // 5 seconds between shots
    
    // Safe zone key mapping for each target letter
    private static readonly Dictionary<char, string[]> SafeZoneKeys = new()
    {
        { 'S', new[] { "S", "Z", "X", "Alt" } },
        { 'H', new[] { "H", "B", "N", "Space" } },
        { 'U', new[] { "U", "H", "J", "N", "M", "Space" } },
        { 'T', new[] { "T", "F", "G", "V", "B", "Space" } },
        { 'D', new[] { "D", "X", "C", "Space" } },
        { 'O', new[] { "O", "K", "L", ",", ".", "Alt", "Space" } },
        { 'W', new[] { "W", "A", "S", "Z", "X", "Alt" } },
        { 'N', new[] { "N", "Space" } }
    };
    
    private int _currentLetterIndex = 0;
    private float _shootTimer = 0f;
    private Key _currentTargetKey;
    private readonly Random _random = new();
    private List<Key> _allKeys;
    
    // Boss movement (reuse from Phase 1)
    private int _movementDirection = 1; // 1 for right, -1 for left
    private const float MovementSpeed = 0.5f; // pixels per frame

    // Properties
    public bool IsComplete => _currentLetterIndex >= SHUTDOWN_SEQUENCE.Length;
    public BossPhase? NextPhase => BossPhase.Phase3_EscapeEnter;
    private char CurrentLetter => SHUTDOWN_SEQUENCE[_currentLetterIndex];
    private List<Key> AllKeys => _allKeys ??= Global.World.Boss?.Keys?.ToList() ?? new List<Key>();

    public void OnEnter(Boss boss, IEnumerable<Key> keys)
    {
        _currentLetterIndex = 0;
        _shootTimer = 0f;
        
        // Register all keys for collision and set retaliation callbacks
        foreach (var key in AllKeys)
        {
            Global.World.CollisionManager.Register(key);
            key.OnInterrupted = () => OnIncorrectKeyHit(key);
        }
        
        StartNextLetter();
    }

    public void Update(GameTime gameTime, Boss boss, IEnumerable<Key> keys)
    {
        if (IsComplete) return;
        
        // Update shoot timer
        _shootTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Check if it's time for the current target key to shoot
        if (_shootTimer >= SHOOT_INTERVAL)
        {
            _currentTargetKey?.ShootBulletFan();
            _shootTimer = 0f; // Reset timer for next cycle
        }
        
        // Update boss horizontal movement (reuse from Phase 1)
        UpdateBossMovement(boss);
    }

    public void OnExit(Boss boss, IEnumerable<Key> keys)
    {
        // Re-register all keys to restore normal collision detection
        foreach (var key in keys)
        {
            Global.World.CollisionManager.Register(key);
            key.GlowIntensity = 0f; // Ensure no keys are glowing
            key.OnInterrupted = null; // Clear callbacks
        }
        
        _currentTargetKey = null;
    }

    private void StartNextLetter()
    {
        if (IsComplete)
            return;
        
        // Find the key for current letter
        _currentTargetKey = FindKeyByCharacter(CurrentLetter, AllKeys);
        if (_currentTargetKey == null)
            return; // Should not happen in a properly configured keyboard
        
        // Set the target key to glow continuously and use correct callback
        _currentTargetKey.GlowIntensity = 1f;
        _currentTargetKey.OnInterrupted = OnTargetKeyHit;
        
        System.Diagnostics.Debug.WriteLine($"Phase2: Target key set to '{CurrentLetter}' (Character: '{_currentTargetKey.Character}')");
        
        // Deregister safe zone keys for bullet passthrough
        DeregisterSafeZoneKeys(CurrentLetter, AllKeys);
        
        // Reset shoot timer for this letter
        _shootTimer = 0f;
    }

    private void OnTargetKeyHit()
    {
        if (_currentTargetKey == null) return;
        
        char currentLetter = SHUTDOWN_SEQUENCE[_currentLetterIndex];
        System.Diagnostics.Debug.WriteLine($"Phase2: Target key '{currentLetter}' was hit! Moving to next letter.");
        
        // Stop current target key glow
        _currentTargetKey.GlowIntensity = 0f;
        _currentTargetKey.OnInterrupted = null;
        
        // Re-register safe zone keys for current letter
        ReregisterSafeZoneKeys(CurrentLetter, AllKeys);
        
        // Move to next letter
        _currentLetterIndex++;
        
        // Start next letter (or complete if done)
        StartNextLetter();
    }
    
    private void DeregisterSafeZoneKeys(char targetLetter, List<Key> keys)
    {
        if (!SafeZoneKeys.TryGetValue(targetLetter, out var safeZoneKeyStrings))
            return;
        
        foreach (var keyString in safeZoneKeyStrings)
        {
            var key = FindKeyByString(keyString, keys);
            if (key != null && key != _currentTargetKey) // Keep target key registered
            {
                System.Diagnostics.Debug.WriteLine($"Phase2: Deregistering safe zone key '{keyString}' (Character: '{key.Character}', KeyText: '{key.KeyText}')");
                
                // Clear callback first to prevent retaliation
                key.OnInterrupted = null;
                
                // Then unregister from collision manager
                Global.World.CollisionManager.UnRegister(key);
            }
        }
    }
    
    private void ReregisterSafeZoneKeys(char targetLetter, List<Key> keys)
    {
        if (!SafeZoneKeys.TryGetValue(targetLetter, out var safeZoneKeyStrings))
            return;
        
        foreach (var keyString in safeZoneKeyStrings)
        {
            var key = FindKeyByString(keyString, keys);
            if (key != null)
            {
                Global.World.CollisionManager.Register(key);
                key.OnInterrupted = () => OnIncorrectKeyHit(key); // Reset to incorrect hit handler
            }
        }
    }
    
    private void OnIncorrectKeyHit(Key hitKey)
    {
        System.Diagnostics.Debug.WriteLine($"Phase2: Incorrect key hit! Key='{hitKey.KeyText}' (Character: '{hitKey.Character}') - Triggering retaliation.");
        
        // Random retaliation: bullet fan or spawn 10 fighters
        if (_random.Next(2) == 0)
        {
            System.Diagnostics.Debug.WriteLine($"Phase2: Retaliation - Bullet fan from target key");
            // Bullet fan retaliation
            _currentTargetKey?.ShootBulletFan();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"Phase2: Retaliation - Spawning 10 fighters");
            // Spawn 10 fighters retaliation
            SpawnFighterRetaliation();
        }
    }
    
    private void SpawnFighterRetaliation()
    {
        const int fighterCount = 10;
        const float screenWidth = 1152f; // Based on game screen width
        const float fighterSpacing = screenWidth / (fighterCount + 1); // Space them evenly
        
        // Spawn above the boss in map coordinates
        float spawnY = Global.World.Boss.MapPosition.Y - 300f; // 300 pixels above boss
        
        System.Diagnostics.Debug.WriteLine($"Phase2: SpawnFighterRetaliation - Starting to spawn {fighterCount} fighters");
        
        for (int i = 0; i < fighterCount; i++)
        {
            float spawnX = fighterSpacing * (i + 1); // Evenly distributed X positions
            
            var fighterInfo = new JetFighterInfo(
                ID: $"Boss_Retaliation_Fighter_{i}_{DateTime.Now.Ticks}",
                X: (int)spawnX,
                Y: (int)spawnY,
                MovementSpeed: 0.5f, // Standard fighter speed
                WakeDistance: 2000f  // Large wake distance for immediate activation
            );
            
            System.Diagnostics.Debug.WriteLine($"Phase2: Spawning fighter {i + 1} at X={spawnX:F0}, Y={spawnY}");
            
            try
            {
                Global.World.JetFighterManager.Register(fighterInfo);
                System.Diagnostics.Debug.WriteLine($"Phase2: Fighter {i + 1} registered successfully");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Phase2: Error registering fighter {i + 1}: {ex.Message}");
            }
        }
        
        System.Diagnostics.Debug.WriteLine($"Phase2: SpawnFighterRetaliation - Completed spawning {fighterCount} fighters");
    }
    
    private Key FindKeyByCharacter(char character, List<Key> keys)
    {
        char upperChar = char.ToUpper(character);
        var matchingKeys = keys.Where(key => char.ToUpper(key.Character) == upperChar).ToList();
        
        // Prefer single-character keys (like "T") over multi-character keys (like "Tab")
        var foundKey = matchingKeys.FirstOrDefault(key => key.KeyText.Length == 1) 
                      ?? matchingKeys.FirstOrDefault();
        
        System.Diagnostics.Debug.WriteLine($"Phase2: FindKeyByCharacter('{character}') found key with Character='{foundKey?.Character}' KeyText='{foundKey?.KeyText}'");
        return foundKey;
    }
    
    private Key FindKeyByString(string keyString, List<Key> keys)
    {
        // First try to match by KeyText (works for special keys like Space, Alt, etc.)
        var keyByText = keys.FirstOrDefault(key => 
            string.Equals(key.KeyText, keyString, StringComparison.OrdinalIgnoreCase));
        
        if (keyByText != null)
            return keyByText;
        
        // Fallback to Character matching for regular letter/number keys
        return keys.FirstOrDefault(key => 
            string.Equals(key.Character.ToString(), keyString, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Updates boss horizontal movement between x=64 and x=1088-896=192
    /// Reused from Phase1ChaosKeys
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