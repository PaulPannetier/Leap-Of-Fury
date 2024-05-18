using System;
using UnityEngine;

public class PeriodicIterruptor : MonoBehaviour
{
    private float timer;

    [SerializeField] private bool startActivated = true;
    [SerializeField] private float delayoffset;
    [SerializeField] private float delayDesactivatedToActivated;
    [SerializeField] private float delayActivatedToDesactivated;

    [HideInInspector] public bool isActivated {  get; private set; }
    [HideInInspector] public Action onActivated;
    [HideInInspector] public Action onDesactivated;

    private void Awake()
    {
        timer = -delayoffset;
        isActivated = startActivated;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (isActivated)
        {
            if(timer > delayActivatedToDesactivated)
            {
                onDesactivated.Invoke();
                isActivated = false;
                timer = 0f;
                print("Desactivated");
            }
        }
        else
        {
            if (timer > delayDesactivatedToActivated)
            {
                onDesactivated.Invoke();
                isActivated = true;
                timer = 0f;
                print("Activated");
            }
        }
    }

    #region OnValidate

#if UNITY_EDITOR

    private void OnValidate()
    {
        delayoffset = Mathf.Max(delayoffset, 0f);
        delayDesactivatedToActivated = Mathf.Max(delayDesactivatedToActivated, 0f);
        delayActivatedToDesactivated = Mathf.Max(delayActivatedToDesactivated, 0f);
    }

#endif

    #endregion
}
