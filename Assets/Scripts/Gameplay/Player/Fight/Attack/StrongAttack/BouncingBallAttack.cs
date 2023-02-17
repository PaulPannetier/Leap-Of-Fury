using System;
using System.Collections;
using UnityEngine;

public class BouncingBallAttack : StrongAttack
{
    private Movement movement;
    private float speed;
    private int nbBalls;

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
        movement = GetComponent<Movement>();
    }

    protected override void Start()
    {
        base.Start();
        speed = initSpeed;
        nbBalls = initNbBalls;
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

        callbackEnd.Invoke();

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

    private void OnValidate()
    {
        nbBounce = Math.Max(0, nbBounce);
        speed = Mathf.Max(0f, speed);
        shootTime = Mathf.Max(0f, shootTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw(transform.position, 2f, -shootAngle * Mathf.Deg2Rad, shootAngle * Mathf.Deg2Rad);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + Useful.Vector2FromAngle(shootAngle * Mathf.Deg2Rad, 2f));
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + Useful.Vector2FromAngle(-shootAngle * Mathf.Deg2Rad, 2f));
        Gizmos.color = Color.red;
        Circle.GizmosDraw(transform.position, shootDistanceFromChar);
    }
}
