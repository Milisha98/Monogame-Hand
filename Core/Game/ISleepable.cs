namespace Hands.Core;
internal interface ISleepable : IMapPosition, IUpdate
{
    public bool IsAsleep { get; }
}
