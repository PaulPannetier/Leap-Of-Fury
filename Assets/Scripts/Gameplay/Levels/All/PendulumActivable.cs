using UnityEngine;

public abstract class PendulumActivable : ActivableObject
{
    [SerializeField] private Pendulum pendulum;
    [SerializeField] private int tickOffset = 0;
    [SerializeField] private int nbTickActivatedToDesactivated, nbTickDesactivatedToActivated;
    private int nbCurrentTick;
    private float lastTickTime = -10f;
    private float pendulumOscillationDuration;

    [HideInInspector] public float activationPercentage => isActivated ? 1f : (float)nbCurrentTick / nbTickDesactivatedToActivated;
    [HideInInspector]
    public float activationPercentageSmooth
    {
        get
        {
            if (isActivated)
                return 1f;

            float maxInter = 1f / nbTickDesactivatedToActivated;
            float inter = Mathf.Lerp(0f, maxInter, (Time.time - lastTickTime) / pendulumOscillationDuration);
            return activationPercentage + inter;
        }
    }

    protected override void Start()
    {
        base.Start();
        nbCurrentTick = -tickOffset;
        lastTickTime = Time.time;
        pendulumOscillationDuration = pendulum.oscilatingDuration;
        pendulum.callbackOnPendulumTick += OnPendulumTick;
    }

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

    #region Destroy/OnValidate

    protected virtual void OnDestroy()
    {
        pendulum.callbackOnPendulumTick -= OnPendulumTick;
    }

#if UNITY_EDITOR

    protected virtual void OnValidate()
    {
        nbTickActivatedToDesactivated = Mathf.Max(0, nbTickActivatedToDesactivated);
        nbTickDesactivatedToActivated = Mathf.Max(0, nbTickDesactivatedToActivated);
        tickOffset = Mathf.Max(tickOffset, 0);
    }

#endif

    #endregion
}
