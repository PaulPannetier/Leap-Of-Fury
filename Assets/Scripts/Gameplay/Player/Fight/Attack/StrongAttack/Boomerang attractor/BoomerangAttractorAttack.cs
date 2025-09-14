using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class BoomrangAttractorAttack : StrongAttack
{
    private LayerMask groundMask;
    private List<BoomerangAttractor> currentAttractors;

    [SerializeField] private BoomerangAttractor attractorPrefabs;
    [SerializeField] private int maxAttractor = 5;
    [SerializeField] private float buildDuration = 0.2f;
    [SerializeField] private float maxDistanceFromGround = 0.5f;

    protected override void Awake()
    {
        base.Awake();
        currentAttractors = new List<BoomerangAttractor>(maxAttractor);
    }

    protected override void Start()
    {
        base.Start();
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
    }

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if (!cooldown.isActive)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        Vector2 groundPos;
        if(!TryGetGroundPosition(out groundPos))
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        StartCoroutine(LaunchCorout(groundPos, callbackEnableOtherAttack, callbackEnableThisAttack));
        return true;
    }

    private IEnumerator LaunchCorout(Vector2 groundPos, Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        yield return PauseManager.instance.Wait(buildDuration);

        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();

        BoomerangAttractor boomerangAttractor = Instantiate(attractorPrefabs);
        groundPos.y += boomerangAttractor.distanceFromGround;
        boomerangAttractor.transform.position = groundPos;
        currentAttractors.Add(boomerangAttractor);
    }

    private bool TryGetGroundPosition(out Vector2 groundPosition)
    {
        ToricRaycastHit2D raycast = PhysicsToric.Raycast(transform.position, Vector2.down, maxDistanceFromGround, groundMask);
        groundPosition = raycast.point;
        return raycast.collider != null;
    }

    public BoomerangAttractor[] GetAttractors() => currentAttractors.ToArray();

    public void OnAttractorReachByBoomerang(BoomerangAttractor attractor)
    {
        currentAttractors.Remove(attractor);
        attractor.OnBeenReachByBoomerang();
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector2 pos = transform.position;
        Gizmos.DrawLine(pos, pos + (maxDistanceFromGround * Vector2.down));
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        maxAttractor = Mathf.Max(0, maxAttractor);
        buildDuration = Mathf.Max(0, buildDuration);
        maxDistanceFromGround = Mathf.Max(0, maxDistanceFromGround);
    }

#endif
}
