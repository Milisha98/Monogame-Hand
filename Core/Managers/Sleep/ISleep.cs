namespace Hands.Core;
internal interface ISleep : IMapPosition, IUpdate
{
    public bool IsAsleep { get; }
    public float WakeDistance { get; }
    public abstract void OnSleep();
    public abstract void OnSisterAwake();
}
