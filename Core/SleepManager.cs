namespace Hands.Core;
internal class SleepManager : IUpdate
{
    private readonly float _triggerDistance;
    private readonly bool _canGoBackToSleep;

    public delegate void SleepHandler();
    public event SleepHandler OnSisterAwake;
    public event SleepHandler OnSleep;

    public SleepManager(ISleepable target, float triggerDistance, bool canGoBackToSleep = true)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
        _triggerDistance = triggerDistance;
        _canGoBackToSleep = canGoBackToSleep;
    }

    public void Update(GameTime gameTime)
    {
        if (DistanceToPlayer > _triggerDistance)
        {
            if (!Target.IsAsleep && _canGoBackToSleep)
            {
                OnSleep?.Invoke();
            }
        }
        else // DistanceToPlayer <= _triggerDistance
        {
            if (Target.IsAsleep)
            {
                OnSisterAwake?.Invoke();
            }
        }
    }

    public ISleepable Target { get; init; }
    public float DistanceToPlayer => Vector2.Distance(Target.MapPosition, Global.World.Camera.Center);

}
