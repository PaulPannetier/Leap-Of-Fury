using UnityEngine;
using static Interruptor;

public abstract class ActivableObject : MonoBehaviour
{
    [SerializeField] private bool startActivated = true;

    [Header("Pendulum")]
    [SerializeField] private bool usePendulum;
    [SerializeField] private Pendulum pendulum;
    [SerializeField] private int nbTickActivatedToDesactivated, nbTickDesactivatedToActivated;
    private int nbCurrentTick;

    [Header("Interruptor")]
    [SerializeField] private bool useByInterruptor;
    [SerializeField] private Interruptor interruptor;

    public bool isActivated {  get; private set; }

    protected virtual void Start()
    {
        nbCurrentTick = 0;

        if (usePendulum)
            pendulum.callbackOnPendulumTick += OnPendulumTick;

        if(useByInterruptor)
        {
            interruptor.onActivate += OnInterruptorActivated;
            interruptor.onDesactivate += OnInterruptorDesactivated;
        }

        isActivated = startActivated;
        if (startActivated)
            OnActivated();
        else
            OnDesactivated();
    }

    protected abstract void OnActivated();
    protected abstract void OnDesactivated();

    #region Pendulum

    private void OnPendulumTick()
    {
        nbCurrentTick++;
        if (isActivated)
        {
            if (nbCurrentTick >= nbTickActivatedToDesactivated)
            {
                isActivated = false;
                nbCurrentTick = 0;
                OnDesactivated();
            }
        }
        else
        {
            if (nbCurrentTick >= nbTickDesactivatedToActivated)
            {
                isActivated = true;
                nbCurrentTick = 0;
                OnActivated();
            }
        }
    }

    #endregion

    #region Interruptor

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

    #endregion

    #region Destroy/OnValidate

    protected virtual void OnDestroy()
    {
        if (usePendulum)
            pendulum.callbackOnPendulumTick -= OnPendulumTick;
    }

#if UNITY_EDITOR

    protected virtual void OnValidate()
    {
        nbTickActivatedToDesactivated = Mathf.Max(0, nbTickActivatedToDesactivated);
        nbTickDesactivatedToActivated = Mathf.Max(0, nbTickDesactivatedToActivated);
    }

#endif

    #endregion

}
