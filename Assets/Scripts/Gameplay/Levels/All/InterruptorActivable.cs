using UnityEngine;
using static Interruptor;

public abstract class InterruptorActivable : ActivableObject
{
    [SerializeField] private bool useByInterruptor;
    [SerializeField] private Interruptor interruptor;

    protected override void Start()
    {
        base.Start();
        interruptor.onActivate += OnInterruptorActivated;
        interruptor.onDesactivate += OnInterruptorDesactivated;
    }

    private void OnInterruptorActivated(PressedInfo pressedInfo)
    {
        isActivated = true;
        OnActivated();
    }

    private void OnInterruptorDesactivated()
    {
        isActivated = false;
        OnDesactivated();
    }

    protected virtual void OnDestroy()
    {
        interruptor.onActivate -= OnInterruptorActivated;
        interruptor.onDesactivate -= OnInterruptorDesactivated;
    }
}
