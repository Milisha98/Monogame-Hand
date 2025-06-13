using System;
using System.Collections.Generic;
using System.Linq;

namespace Hands.Core.Animation;
public class Workflow<T>
{
    private readonly WorkflowStage<T>[] _stages;
    private int _stageIndex = -1;
    private Tween _tween;
    private bool _isInitialized = false;

    public delegate void WorkflowCompletedHandler();
    public delegate void StateChangedHandler(T newState);
    public event WorkflowCompletedHandler OnCompleted;
    public event StateChangedHandler OnStateChanged;

    public Workflow(WorkflowStage<T>[] stages)
    {
        if (stages?.Length == 0)
        {
            throw new ArgumentNullException(nameof(stages), $"{nameof(stages)} must contain at least one element.");
        }
        _stages = stages;

    }

    public void Update(GameTime gameTime)
    {
        if (!IsActive) return;
        if (!_isInitialized)
        {
            ChangeStage();
            _isInitialized = true;
        }

        if (_tween.IsComplete)
        {
            if (_stageIndex < _stages.Length - 1)
            {
                ChangeStage();
            }
            else
            {
                IsComplete = true;
                OnCompleted?.Invoke();
            }

        }
        else
        {
            float pct = _tween.Update(gameTime);
            CurrentPercent = pct;
        }
    }

    public void Reset()
    {
        _stageIndex = 0;
        _isInitialized = false;
        IsComplete = false;
        CurrentPercent = 0f;
        _tween = new Tween(CurrentStage.Duration);
    }

    private void ChangeStage()
    {
        CurrentPercent = 0f;
        _stageIndex++;
        _tween = new Tween(CurrentStage.Duration);
        OnStateChanged?.Invoke(CurrentState);

    }

    public bool IsComplete { get; private set; } = false;
    public WorkflowStage<T> CurrentStage => _stages[_stageIndex];
    public T CurrentState => CurrentStage.State;
    public float CurrentPercent { get; private set; } = 0f;
    public bool IsActive { get; set; } = true;
}


public record struct WorkflowStage<T>(T State, TimeSpan Duration);