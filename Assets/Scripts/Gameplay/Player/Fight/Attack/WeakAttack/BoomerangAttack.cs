using System;
using UnityEngine;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;
using static Boomerang;

public class BoomerangAttack : WeakAttack
{
    private Movement movement;
    private Boomerang currentBoomerang;

    [SerializeField] private GameObject boomerangPrefab;
    [SerializeField] private float distanceToInstantiate = 0.4f;
    [SerializeField] private AnimationCurve speedCurvePhase1, accelerationCurvePhase2;
    [SerializeField] private float maxSpeedPhase1, maxSpeedPhase2, durationPhase1, accelerationDurationPhase2;
    [SerializeField] private float recuperationRange;

#if UNITY_EDITOR

    [SerializeField] private bool drawGizmos = true;

#endif

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<Movement>();
    }


    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if (!cooldown.isActive)
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
        return new BoomerangLaunchData(dir, speedCurvePhase1, accelerationCurvePhase2, maxSpeedPhase1, durationPhase1,
            accelerationDurationPhase2, this, maxSpeedPhase2, recuperationRange);
    }

    public void GetBack()
    {
        currentBoomerang = null;
    }

#if UNITY_EDITOR

    protected void OnValidate()
    {
        durationPhase1 = Mathf.Max(0f, durationPhase1);
        accelerationDurationPhase2 = Mathf.Max(0f, accelerationDurationPhase2);
        maxSpeedPhase1 = Mathf.Max(0f, maxSpeedPhase1);
        maxSpeedPhase2 = Mathf.Max(0f, maxSpeedPhase2);
        distanceToInstantiate = Mathf.Max(0f, distanceToInstantiate);
        recuperationRange = Mathf.Max(0f, recuperationRange);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Gizmos.color = Color.green;
        Circle.GizmosDraw(transform.position, recuperationRange);
        Gizmos.color = Color.black;
        Circle.GizmosDraw(transform.position, distanceToInstantiate);
    }

#endif
}
