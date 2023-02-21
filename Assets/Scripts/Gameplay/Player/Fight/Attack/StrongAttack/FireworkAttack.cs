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
}
