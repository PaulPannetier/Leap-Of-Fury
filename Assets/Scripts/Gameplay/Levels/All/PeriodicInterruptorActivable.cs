using UnityEngine;

public abstract class PeriodicInterruptorActivable : ActivableObject
{
    [SerializeField] protected PeriodicIterruptor periodicIterruptor;

    protected override void Start()
    {
        base.Start();
        periodicIterruptor.onActivated += OnActivated;
        periodicIterruptor.onDesactivated += OnDesactivated;
    }

    protected virtual void OnDestroy()
    {
        periodicIterruptor.onActivated -= OnActivated;
        periodicIterruptor.onDesactivated -= OnDesactivated;
    }
}
