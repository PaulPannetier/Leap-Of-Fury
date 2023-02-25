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
        float angleStep = (fireworkDiffusionAngle / nbFireworkLaunch) * Mathf.Deg2Rad;
        float begAngle = angle - fireworkDiffusionAngle * 0.5f * Mathf.Deg2Rad;

        for (int i = 0; i < nbFireworkLaunch; i++)
        {
            float fireworkAngle = begAngle + i * angleStep;
            Vector2 fireworkPos = (Vector2)transform.position + Useful.Vector2FromAngle(fireworkAngle, distanceFromCharWhenLauch);
            Firework firework = Instantiate(fireworkPrefaps, fireworkPos, Quaternion.Euler(0f, 0f, fireworkAngle), CloneParent.cloneParent);
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

    //to rm
    [SerializeField] private LayerMask groundMaskToRm;
    private float angleTest = 0f;
    private void OnDrawGizmosSelected()
    {
        //test overlap capsule
        if (Application.isPlaying)
        {
            Vector2 mousePos = Useful.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 size = new Vector2(1.5f, 2.5f);
            Capsule c = new Capsule(mousePos, size);
            if(CustomInput.GetKey(KeyCode.A))
            {
                angleTest -= 1f;
            }
            if (CustomInput.GetKey(KeyCode.E))
            {
                angleTest += 1f;
            }

            angleTest = Useful.ClampModulo(0f, 360f, angleTest);
            c.Rotate(angleTest * Mathf.Deg2Rad);
            //print(angleTest + " =?= " + c.AngleHori() * Mathf.Rad2Deg);
            Gizmos.color = Color.green;
            Capsule.GizmosDraw(c);

            Hitbox h1 = (Hitbox)c.hitbox.Clone();
            Gizmos.color = Color.yellow;
            Hitbox.GizmosDraw(h1);

            bool b = PhysicsToric.OverlapCapsule(c, groundMaskToRm) == null;
            Gizmos.color = b ? Color.green : Color.red;
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
