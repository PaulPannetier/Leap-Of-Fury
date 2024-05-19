using UnityEngine;

public abstract class PeriodicInterruptorActivable : ActivableObject
{
    [SerializeField] protected PeriodicIterruptor periodicIterruptor;

    protected override void Start()
    {
        base.Start();
        periodicIterruptor.onActivated += OnActivatedInternal;
        periodicIterruptor.onDesactivated += OnDesactivatedInternal;
    }

    private void OnActivatedInternal()
    {
        OnActivated();
        isActivated = true;
    }

    private void OnDesactivatedInternal()
    {
        OnDesactivated();
        isActivated = false;
    }

    protected virtual void OnDestroy()
    {
        periodicIterruptor.onActivated -= OnActivatedInternal;
        periodicIterruptor.onDesactivated -= OnDesactivatedInternal;
    }
}
