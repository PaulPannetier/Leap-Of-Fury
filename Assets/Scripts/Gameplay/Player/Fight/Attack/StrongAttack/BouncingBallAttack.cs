using System;
using System.Collections;
using UnityEngine;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;

public class BouncingBallAttack : StrongAttack
{
    private CharacterController movement;
    private float speed;
    private int nbBalls;

    [SerializeField] private bool drawGizmos;
    [SerializeField] private float shootDistanceFromChar = 0.4f;
    [SerializeField] private float initSpeed = 7f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField, Range(0f, 1f)] private float speedVariation = 0.2f;
    [SerializeField] private float maxBallDuration = 2f;
    [SerializeField] private int nbBounce = 3;
    [SerializeField] private int initNbBalls = 2;
    [SerializeField] private int maxNbBalls = 10;
    [SerializeField, Range(0f, 180f)] private float shootAngle;
    [SerializeField, Tooltip("The time (sec) between 2 balls in a shot")] private float shootTime;
    [SerializeField] private GameObject bouncingBallPrefabs;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<CharacterController>();
    }

    protected override void Start()
    {
        base.Start();
        speed = initSpeed;
        nbBalls = initNbBalls;
    }

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if (!cooldown.isActive)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }
        base.Launch(callbackEnableOtherAttack, callbackEnableThisAttack);

        cooldown.Reset();
        StartCoroutine(DoBounceAttack(callbackEnableOtherAttack, callbackEnableThisAttack));
        return true;
    }

    private IEnumerator DoBounceAttack(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        float timeCounter = 0f;
        while (timeCounter < castDuration)
        {
            yield return null;
            if (!PauseManager.instance.isPauseEnable)
            {
                timeCounter += Time.deltaTime;
            }
        }

        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();

        Vector2 dir = movement.GetCurrentDirection(true);
        float dirAngle = Useful.AngleHori(Vector2.zero, dir);

        for (int i = 0; i < nbBalls; i++)
        {
            float randAngle = Random.Rand(-shootAngle, shootAngle) * Mathf.Deg2Rad;
            Vector2 randDir = Useful.Vector2FromAngle(randAngle + dirAngle);
            float ballSpeed = speed * (1f + Random.Rand(-speedVariation, speedVariation));
            GameObject ball = Instantiate(bouncingBallPrefabs, (Vector2)transform.position + randDir * shootDistanceFromChar, Quaternion.identity, CloneParent.cloneParent);
            BouncingBall bb = ball.GetComponent<BouncingBall>();
            bb.Launch(randDir, ballSpeed, nbBounce, maxBallDuration, this);

            yield return Useful.GetWaitForSeconds(shootTime);
        }
    }

    public void PickUpFiole(Fiole fiole)
    {
        speed = Mathf.Min(speed * 1.1f, maxSpeed);
        nbBalls = Math.Min(maxNbBalls, nbBalls + 1);
    }

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        nbBounce = Math.Max(0, nbBounce);
        speed = Mathf.Max(0f, speed);
        shootTime = Mathf.Max(0f, shootTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Gizmos.color = Color.green;
        Circle.GizmosDraw(transform.position, 2f, -shootAngle * Mathf.Deg2Rad, shootAngle * Mathf.Deg2Rad);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + Useful.Vector2FromAngle(shootAngle * Mathf.Deg2Rad, 2f));
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + Useful.Vector2FromAngle(-shootAngle * Mathf.Deg2Rad, 2f));
        Gizmos.color = Color.red;
        Circle.GizmosDraw(transform.position, shootDistanceFromChar);
    }

#endif
}
