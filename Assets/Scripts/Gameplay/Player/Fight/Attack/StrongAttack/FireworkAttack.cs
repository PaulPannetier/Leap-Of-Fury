using System;
using UnityEngine;
#if UNITY_EDITOR
using Collision2D;
using Collider2D = UnityEngine.Collider2D;
#endif

public class FireworkAttack : StrongAttack
{
    private Rigidbody2D rb;
    private Movement movement;

    [SerializeField] private float bumpVelocity = 2f;
    [SerializeField] private int nbFireworkLaunch = 3;
    [SerializeField, Range(0f, 360f)] private float fireworkDiffusionAngle = 90f;
    [SerializeField] private float distanceFromCharWhenLauch = 0.2f;
    [SerializeField] private Firework fireworkPrefaps;

#if UNITY_EDITOR

    [SerializeField] private bool drawGizmos = true;

#endif

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
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
        cooldown.Reset();

        LaunchFirework();

        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
        base.Launch(callbackEnableOtherAttack, callbackEnableThisAttack);

        return true;
    }

    private void LaunchFirework()
    {
        Vector2 dir = -movement.GetCurrentDirection(true);
        float angle = Useful.AngleHori(Vector2.zero, dir);
        float angleStep = nbFireworkLaunch <= 1 ? 0f : (fireworkDiffusionAngle / (nbFireworkLaunch - 1)) * Mathf.Deg2Rad;
        float begAngle = nbFireworkLaunch <= 1 ? angle : angle - fireworkDiffusionAngle * 0.5f * Mathf.Deg2Rad;

        for (int i = 0; i < nbFireworkLaunch; i++)
        {
            float fireworkAngle = begAngle + i * angleStep;
            Vector2 fireworkPos = (Vector2)transform.position + Useful.Vector2FromAngle(fireworkAngle, distanceFromCharWhenLauch);
            Firework firework = Instantiate(fireworkPrefaps, fireworkPos, Quaternion.Euler(0f, 0f, fireworkAngle * Mathf.Rad2Deg), CloneParent.cloneParent);
            firework.Launch(fireworkAngle, playerCommon, this);
        }

        rb.velocity = -bumpVelocity * dir;
    }

    public void OnFireworkTouchEnnemy(Firework firework, GameObject ennemy)
    {
        OnTouchEnemy(ennemy);
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        bumpVelocity = Mathf.Max(bumpVelocity, 0f);
        nbFireworkLaunch = Mathf.Max(nbFireworkLaunch, 0);
        distanceFromCharWhenLauch = Mathf.Max(distanceFromCharWhenLauch, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Gizmos.color = Color.green;
        float a1 = (270f + fireworkDiffusionAngle * 0.5f) * Mathf.Deg2Rad;
        float a2 = (270f - fireworkDiffusionAngle * 0.5f) * Mathf.Deg2Rad;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + Useful.Vector2FromAngle(a1));
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + Useful.Vector2FromAngle(a2));
        Circle.GizmosDraw(transform.position, 1f, a2, a1);
    }

#endif
}
