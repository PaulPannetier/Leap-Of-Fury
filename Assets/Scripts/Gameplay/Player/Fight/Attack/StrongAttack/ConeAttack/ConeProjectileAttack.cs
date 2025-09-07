using System;
using System.Collections;
using UnityEngine;
using Collision2D;

public class ConeProjectileAttack : StrongAttack
{
    private CharacterController charController;
    private bool isLaunchingProjectile = false;
    private Vector2 inputDir;
    [SerializeField] private byte remainingProjectiles;

#if UNITY_EDITOR
    [SerializeField] private bool drawGizmos;
#endif

    [SerializeField] private byte nbProjectile;
    [SerializeField, Range(0f, 360f)] private float coneAngle;
    [SerializeField] private float coneRandomAngleVariation;
    [SerializeField] private ConeProjectile projectilePrefabs;
    [SerializeField] private float bumpForce;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float instanciateDistance;
    [SerializeField] private float delayBetweenProjectiles;
    [SerializeField] private float castDuration;
    [SerializeField] private bool useOnly8Dir = true;

    protected override void Awake()
    {
        base.Awake();
        charController = GetComponent<CharacterController>();
        remainingProjectiles = nbProjectile;
    }

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if (!cooldown.isActive || isLaunchingProjectile || remainingProjectiles <= 0)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        StartCoroutine(OnLaunch(callbackEnableOtherAttack, callbackEnableThisAttack));
        return true;
    }

    private IEnumerator OnLaunch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        isLaunchingProjectile = true;
        inputDir = charController.GetCurrentDirection(useOnly8Dir);
        charController.Freeze();

        yield return PauseManager.instance.Wait(castDuration);

        yield return InstanciateProjectiles();

        charController.UnFreeze();

        cooldown.Reset();
        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
        isLaunchingProjectile = false;

        charController.ApplyBump(-bumpForce * inputDir);
    }

    private IEnumerator InstanciateProjectiles()
    {
        float[] angles = new float[remainingProjectiles];
        float midAngle = Useful.AngleHori(Vector2.zero, inputDir);

        if (remainingProjectiles.IsOdd())
        {
            angles[angles.Length >> 1] = midAngle;
            if(remainingProjectiles > 1)
            {
                int end = (remainingProjectiles - 1) >> 1;
                float angleStep = (coneAngle * Mathf.Deg2Rad) / (remainingProjectiles - 1);
                for (int i = 0; i < end; i++)
                {
                    float angleOffset = (end - i) * angleStep;

                    float lowAngle = Useful.WrapAngle(midAngle - angleOffset);
                    float highAngle = Useful.WrapAngle(midAngle + angleOffset);

                    angles[i] = lowAngle;
                    angles[angles.Length - 1 - i] = highAngle;
                }
            }
        }
        else
        {
            float startAngle = Useful.WrapAngle(midAngle - (coneAngle * Mathf.Deg2Rad * 0.5f));
            float angleStep = coneAngle * Mathf.Deg2Rad / (remainingProjectiles - 1);
            for(int i = 0; i < angles.Length; i++)
            {
                angles[i] = startAngle + (i * angleStep);
            }
        }

        for (int i = 0; i < angles.Length; i++)
        {
            float angleVariationRad = coneRandomAngleVariation * Mathf.Deg2Rad;
            float angle = angles[i] + Random.Rand(-angleVariationRad, angleVariationRad);
            Vector2 dir = Useful.Vector2FromAngle(angle);
            Vector2 projectilePosition = (Vector2)transform.position + (instanciateDistance * dir);
            ConeProjectile coneProjectile = Instantiate(projectilePrefabs, projectilePosition, Quaternion.identity, CloneParent.cloneParent);
            coneProjectile.Launch(projectileSpeed, dir, this);
            if (i != angles.Length - 1)
                yield return PauseManager.instance.Wait(delayBetweenProjectiles);
        }
        remainingProjectiles = 0;
    }

    public void PickProjectile(ConeProjectile projectile)
    {
        remainingProjectiles++;
    }

    public void OnProjectileTouchPlayer(GameObject player)
    {
        base.OnTouchEnemy(player, damageType);
    }

    #region Gizmos/OnValidate

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        nbProjectile = (byte)Mathf.Max(nbProjectile, 1);
        bumpForce = Mathf.Max(bumpForce, 0f);
        projectileSpeed = Mathf.Max(projectileSpeed, 0f);
        castDuration = Mathf.Max(castDuration, 0f);
        instanciateDistance = Mathf.Max(instanciateDistance, 0f);
        delayBetweenProjectiles = Mathf.Max(delayBetweenProjectiles, 0f);
        coneRandomAngleVariation = Mathf.Max(coneRandomAngleVariation, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        if(!drawGizmos)
            return;

        Circle.GizmosDraw(transform.position, instanciateDistance, Color.black);
    }

#endif

    #endregion
}
