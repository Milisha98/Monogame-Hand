using System.Collections.Generic;

namespace Hands.GameObjects.Enemies.Boss;

public enum BossPhase
{
    Phase1_ChaosKeys,
    Phase2_SpellShutdown,
    Phase3_EscapeEnter
}

public interface IBossPhase
{
    /// <summary>
    /// Called when this phase becomes active
    /// </summary>
    void OnEnter(Boss boss, IEnumerable<Key> keys);
    
    /// <summary>
    /// Called every frame while this phase is active
    /// </summary>
    void Update(GameTime gameTime, Boss boss, IEnumerable<Key> keys);
    
    /// <summary>
    /// Called when this phase ends
    /// </summary>
    void OnExit(Boss boss, IEnumerable<Key> keys);
    
    /// <summary>
    /// Returns true when this phase should transition to the next phase
    /// </summary>
    bool IsComplete { get; }
    
    /// <summary>
    /// The next phase to transition to when this phase completes
    /// </summary>
    BossPhase? NextPhase { get; }
}