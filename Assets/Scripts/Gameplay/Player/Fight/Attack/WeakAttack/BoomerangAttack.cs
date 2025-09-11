using System;
using UnityEngine;
using Collision2D;
using static Boomerang;

public class BoomerangAttack : WeakAttack
{
    private CharacterController movement;
    private Boomerang currentBoomerang;
    private BoomrangAttractorAttack boomrangAttractorAttack;

#if UNITY_EDITOR
    [SerializeField] private bool drawGizmos = true;
#endif

    [SerializeField] private GameObject boomerangPrefab;
    [SerializeField] private float distanceToInstantiate = 0.4f;
    [SerializeField] private AnimationCurve speedCurvePhase1, speedCurvePhase2;
    [SerializeField] private float maxSpeedPhase1, maxSpeedPhase2, durationPhase1, accelerationDurationPhase2;
    [SerializeField] private float recuperationRange;

    [Header("PathFinding")]
    [SerializeField] private float minDelayBetweenPathfinfindSearch = 0.5f;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<CharacterController>();
        currentBoomerang = null;
    }

    protected override void Start()
    {
        base.Start();
        boomrangAttractorAttack = GetComponent<BoomrangAttractorAttack>();
    }

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if (!cooldown.isActive || currentBoomerang != null)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        Vector2 dir = movement.GetCurrentDirection(true);
        Vector2 pos = PhysicsToric.GetPointInsideBounds((Vector2)transform.position + dir * distanceToInstantiate);
        currentBoomerang = Instantiate(boomerangPrefab, pos, Quaternion.identity, CloneParent.cloneParent).GetComponent<Boomerang>();

        currentBoomerang.Launch(CreateLaunchData(dir));

        cooldown.Reset();

        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
        return true;
    }

    private BoomerangLaunchData CreateLaunchData(in Vector2 dir)
    {
        return new BoomerangLaunchData(dir, speedCurvePhase1, speedCurvePhase2, maxSpeedPhase1, durationPhase1,
            accelerationDurationPhase2, this, maxSpeedPhase2, recuperationRange, minDelayBetweenPathfinfindSearch, boomrangAttractorAttack.GetAttractors());
    }

    public void GetBack()
    {
        currentBoomerang = null;
    }

    public void OnTouchEnemy(GameObject enemy)
    {
        base.OnTouchEnemy(enemy, damageType);
    }

    #region Gizmos/OnValidate

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        durationPhase1 = Mathf.Max(0f, durationPhase1);
        accelerationDurationPhase2 = Mathf.Max(0f, accelerationDurationPhase2);
        maxSpeedPhase1 = Mathf.Max(0f, maxSpeedPhase1);
        maxSpeedPhase2 = Mathf.Max(0f, maxSpeedPhase2);
        distanceToInstantiate = Mathf.Max(0f, distanceToInstantiate);
        recuperationRange = Mathf.Max(0f, recuperationRange);
        minDelayBetweenPathfinfindSearch = Mathf.Max(minDelayBetweenPathfinfindSearch, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Circle.GizmosDraw(transform.position, recuperationRange, Color.green);
        Circle.GizmosDraw(transform.position, distanceToInstantiate, Color.black);
    }

#endif

    #endregion
}
