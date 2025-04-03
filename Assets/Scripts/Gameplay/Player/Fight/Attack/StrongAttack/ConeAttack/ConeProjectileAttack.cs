using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Collision2D;

public class ConeProjectileAttack : StrongAttack
{
    private CharacterController charController;
    private bool isLaunchingProjectile = false;
    private Vector2 inputDir;
    private int nbProjectilePick;
    private List<ConeProjectile> currentProjectiles;

#if UNITY_EDITOR
    [SerializeField] private bool drawGizmos;
#endif

    [SerializeField] private byte nbProjectile;
    [SerializeField, Range(0f, 360f)] private float coneAngle;
    [SerializeField] private ConeProjectile projectilePrefabs;
    [SerializeField] private float bumpForce;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float instanciateDistance;
    [SerializeField] private float delayBetweenProjectiles;
    [SerializeField] private float[] speedBonusPerPickProjectile;
    [SerializeField] private float castDuration;
    [SerializeField] private bool useOnly8Dir = true;

    protected override void Awake()
    {
#if UNITY_EDITOR || ADVANCE_DEBUG
        if(speedBonusPerPickProjectile.Length < nbProjectile)
        {
            string errorMsg = $"speedBonusPerPickProjectile must have a length  >= at nbProjectile, but got {speedBonusPerPickProjectile.Length} < {nbProjectile}";
            Debug.LogError(errorMsg);
            LogManager.instance.AddLog(errorMsg, speedBonusPerPickProjectile, nbProjectile);
        }
#endif

        base.Awake();
        charController = GetComponent<CharacterController>();
        nbProjectilePick = 0;
        currentProjectiles = new List<ConeProjectile>(nbProjectile);
    }

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if (!cooldown.isActive || isLaunchingProjectile)
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

        for (int i = currentProjectiles.Count - 1; i >= 0; i--)
        {
            currentProjectiles[i].OnAttackReLaunch();
        }

        yield return InstanciateProjectiles();

        nbProjectilePick = 0;

        charController.UnFreeze();

        cooldown.Reset();
        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
        isLaunchingProjectile = false;

        charController.ApplyBump(-bumpForce * inputDir);
    }

    private IEnumerator InstanciateProjectiles()
    {
        float[] angles = new float[nbProjectile];
        float midAngle = Useful.AngleHori(Vector2.zero, inputDir);

        if (nbProjectile.IsOdd())
        {
            angles[angles.Length >> 1] = midAngle;
            if(nbProjectile > 1)
            {
                int end = (nbProjectile - 1) >> 1;
                float angleStep = (coneAngle * Mathf.Deg2Rad) / (nbProjectile - 1);
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
            float angleStep = coneAngle * Mathf.Deg2Rad / (nbProjectile - 1);

            for(int i = 0; i < angles.Length; i++)
            {
                angles[i] = startAngle + (i * angleStep);
            }
        }

        float speedBonus = 1f + (nbProjectilePick > 0 ? speedBonusPerPickProjectile[nbProjectilePick - 1] : 0f);
        float projSpeed = speedBonus * projectileSpeed;
        for (int i = 0; i < angles.Length; i++)
        {
            float angle = angles[i];
            Vector2 dir = Useful.Vector2FromAngle(angle);
            Vector2 projectilePosition = (Vector2)transform.position + (instanciateDistance * dir);
            ConeProjectile coneProjectile = Instantiate(projectilePrefabs, projectilePosition, Quaternion.identity, CloneParent.cloneParent);
            currentProjectiles.Add(coneProjectile);
            coneProjectile.Launch(projSpeed, dir, this);
            if (i != angles.Length - 1)
                yield return PauseManager.instance.Wait(delayBetweenProjectiles);
        }
    }

    public void PickProjectile(ConeProjectile projectile)
    {
        nbProjectilePick++;
    }

    public void OnProjectileDestroy(ConeProjectile projectile)
    {
        currentProjectiles.Remove(projectile);
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
