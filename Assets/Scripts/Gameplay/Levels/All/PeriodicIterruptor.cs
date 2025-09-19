using System;
using UnityEngine;

public class PeriodicIterruptor : MonoBehaviour
{
    private float lastTimeChangeState = 10f;
    private float timer;
    private Animator animator;
    private int animActivated, animInactivated;

    [SerializeField] private bool startActivated = true;
    [SerializeField] private float delayoffset;
    [SerializeField] private float delayDesactivatedToActivated;
    [SerializeField] private float delayActivatedToDesactivated;
    [SerializeField] private float animationStartSpeed = 1f, animationEndSpeed = 10f;

    [HideInInspector] public bool isActivated {  get; private set; }
    [HideInInspector] public Action onActivated;
    [HideInInspector] public Action onDesactivated;

    private void Awake()
    {
        timer = -delayoffset;
        isActivated = startActivated;
        animator = GetComponent<Animator>();
        animActivated = Animator.StringToHash("green");
        animInactivated = Animator.StringToHash("red");
        lastTimeChangeState = Time.time;
    }

    private void Start()
    {
        animator.CrossFade(startActivated ? animActivated : animInactivated, 0f, 0);
    }

    private void Update()
    {
        if (PauseManager.instance.isPauseEnable)
            return;

        timer += Time.deltaTime;
        float delayDuration = isActivated ? delayActivatedToDesactivated : delayDesactivatedToActivated;
        animator.speed = Mathf.Lerp(animationStartSpeed, animationEndSpeed, Mathf.Clamp01((Time.time - lastTimeChangeState) / delayDuration));

        if (isActivated)
        {
            if(timer > delayActivatedToDesactivated)
            {
                onDesactivated.Invoke();
                isActivated = false;
                animator.CrossFade(animInactivated, 0f, 0);
                timer = 0f;
                lastTimeChangeState = Time.time;
            }
        }
        else
        {
            if (timer > delayDesactivatedToActivated)
            {
                onActivated.Invoke();
                isActivated = true;
                animator.CrossFade(animActivated, 0f, 0);
                timer = 0f;
                lastTimeChangeState = Time.time;
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
        animationStartSpeed = Mathf.Max(animationStartSpeed, 0f);
        animationEndSpeed = Mathf.Max(animationEndSpeed, 0f);
    }

#endif

    #endregion
}
