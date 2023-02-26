using System;
using UnityEngine;

public class FireworkAttack : StrongAttack
{
    private Rigidbody2D rb;
    private Movement movement;

    [SerializeField] private float bumpForce = 150f;
    [SerializeField] private int nbFireworkLaunch = 3;
    [SerializeField, Range(0f, 360f)] private float fireworkDiffusionAngle = 90f;
    [SerializeField] private float distanceFromCharWhenLauch = 0.2f;
    [SerializeField] private Firework fireworkPrefaps;

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
        base.Launch(callbackEnableOtherAttack, callbackEnableThisAttack);
        cooldown.Reset();

        LaunchFirework();

        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
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

        rb.AddForce(-dir * bumpForce, ForceMode2D.Impulse);
    }

    public void OnFireworkTouchEnnemy(Firework firework, GameObject ennemy)
    {
        OnTouchEnemy(ennemy);
    }

    private void OnValidate()
    {
        bumpForce = Mathf.Max(bumpForce, 0f);
        nbFireworkLaunch = Mathf.Max(nbFireworkLaunch, 0);
        distanceFromCharWhenLauch = Mathf.Max(distanceFromCharWhenLauch, 0f);
    }

    [SerializeField] private LayerMask groundMask;
    private float tmpAngle = 0f;
    //to rm
    private void OnDrawGizmosSelected()
    {
        //test overlap capsuleAll
        if(Application.isPlaying)
        {
            if(CustomInput.GetKey(KeyCode.A))
            {
                tmpAngle--;
            }
            if (CustomInput.GetKey(KeyCode.E))
            {
                tmpAngle++;
            }
            tmpAngle = Useful.ClampModulo(0f, 360f, tmpAngle);

            Vector2 mousePos = Useful.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Capsule c = new Capsule(mousePos, new Vector2(1, 2), CapsuleDirection2D.Vertical);
            c.Rotate(tmpAngle * Mathf.Deg2Rad);

            //Collider2D[] res = PhysicsToric.OverlapCapsuleAll(c, groundMask);
            Collider2D[] res = Physics2D.OverlapBoxAll(c.hitbox.center, c.hitbox.size, c.hitbox.AngleHori() * Mathf.Rad2Deg, groundMask);
            res.Merge(Physics2D.OverlapCircleAll(c.c1.center, c.c1.radius, groundMask));
            res.Merge(Physics2D.OverlapCircleAll(c.c2.center, c.c2.radius, groundMask));

            Gizmos.color = res.Length > 1 ? Color.red : (res.Length == 1 ? Color.yellow : Color.green);
            Capsule.GizmosDraw(c);
        }

        Gizmos.color = Color.green;
        float a1 = (270f + fireworkDiffusionAngle * 0.5f) * Mathf.Deg2Rad;
        float a2 = (270f - fireworkDiffusionAngle * 0.5f) * Mathf.Deg2Rad;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + Useful.Vector2FromAngle(a1));
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + Useful.Vector2FromAngle(a2));
        Circle.GizmosDraw(transform.position, 1f, a2, a1);
    }
}
