using UnityEngine;
using static Interruptor;

public abstract class ActivableObject : MonoBehaviour
{
    [SerializeField] protected bool startActivated = true;

    [Header("Pendulum")]
    [SerializeField] private bool usePendulum;
    [SerializeField] private Pendulum pendulum;
    [SerializeField] private int nbTickActivatedToDesactivated, nbTickDesactivatedToActivated;
    private int nbCurrentTick;
    private float lastTickTime = -10f;
    private float pendulumOscillationDuration;

    [Header("Interruptor")]
    [SerializeField] private bool useByInterruptor;
    [SerializeField] private Interruptor interruptor;

    public bool isActivated {  get; private set; }
    [HideInInspector] public float activationPercentage => isActivated ? 1f : (float)nbCurrentTick / nbTickDesactivatedToActivated;
    [HideInInspector]
    public float activationPercentageSmooth
    {
        get
        {
            if (isActivated)
                return 1f;

            float maxInter = 1f / nbTickDesactivatedToActivated;
            float inter =  Mathf.Lerp(0f, maxInter, (Time.time - lastTickTime) / pendulumOscillationDuration);
            return activationPercentage + inter;
        }
    }

    protected virtual void Start()
    {
        nbCurrentTick = 0;
        lastTickTime = Time.time;
        if (usePendulum)
        {
            pendulumOscillationDuration = pendulum.oscilatingDuration;
            pendulum.callbackOnPendulumTick += OnPendulumTick;
        }

        if (useByInterruptor)
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
        lastTickTime = Time.time;
        pendulumOscillationDuration = pendulum.oscilatingDuration;
        if (isActivated)
        {
            if (nbCurrentTick >= nbTickActivatedToDesactivated)
            {
                nbCurrentTick = 0;
                OnDesactivated();
                isActivated = false;
            }
        }
        else
        {
            if (nbCurrentTick >= nbTickDesactivatedToActivated)
            {
                nbCurrentTick = 0;
                OnActivated();
                isActivated = true;
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

        if (useByInterruptor)
        {
            interruptor.onActivate += OnInterruptorActivated;
            interruptor.onDesactivate += OnInterruptorDesactivated;
        }
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
