using System;
using System.Collections;
using UnityEngine;

public class BouncingBallAttack : StrongAttack
{
    private Movement movement;
    private int maxBounce;
    private float speed;

    [SerializeField] private float initSpeed = 7f;
    [SerializeField] private float maxSpeed = 7f;
    [SerializeField] private float maxDuration = 2f;
    [SerializeField] private int initMaxBounce = 1;
    [SerializeField] private GameObject bouncingBallPrefabs;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<Movement>();
    }

    protected override void Start()
    {
        base.Start();
        maxBounce = initMaxBounce;
        speed = initSpeed;
    }

    public override bool Launch(Action callbackEnd)
    {
        if (!cooldown.isActive)
        {
            callbackEnd.Invoke();
            return false;
        }
        base.Launch(callbackEnd);
        cooldown.Reset();
        StartCoroutine(DoBounceAttack(callbackEnd));
        return true;
    }

    private IEnumerator DoBounceAttack(Action callbackEnd)
    {
        yield return Useful.GetWaitForSeconds(castDuration);

        GameObject ball = Instantiate(bouncingBallPrefabs, transform.position, Quaternion.identity, CloneParent.cloneParent);
        BouncingBall bb = ball.GetComponent<BouncingBall>();
        bb.Launch(movement.GetCurrentDirection(), speed, maxBounce, maxDuration, this);

        callbackEnd.Invoke();
    }

    public void PickUpFiole(Fiole fiole)
    {
        maxBounce++;
        speed = Mathf.Min(speed * 1.1f, maxSpeed);
    }
}
