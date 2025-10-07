using Hands.Core.Animation;

namespace Hands.GameObjects.Enemies.Boss;

/// <summary>
/// Helper class to track individual glowing key data including workflow state and interruption status
/// </summary>
public class GlowingKeyData
{
    public Key Key { get; set; }
    public Workflow<KeyGlowState> Workflow { get; set; }
    public bool WasInterrupted { get; set; }

    public GlowingKeyData(Key key, Workflow<KeyGlowState> workflow)
    {
        Key = key;
        Workflow = workflow;
        WasInterrupted = false;
    }
}