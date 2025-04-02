using System;
using System.Collections;
using UnityEngine;
using Collision2D;

public class ConeProjectileAttack : StrongAttack
{
    private CharacterController charController;
    private bool isLaunchingProjectile = false;
    private Vector2 inputDir;

#if UNITY_EDITOR
    [SerializeField] private bool drawGizmos;
#endif

    [SerializeField] private int nbProjectile;
    [SerializeField, Range(0f, 360f)] private float coneAngle;
    [SerializeField] private ConeProjectile projectilePrefabs;
    [SerializeField] private float bumpForce;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float instanciateDistance;
    [SerializeField] private float castDuration;
    [SerializeField] private bool useOnly8Dir = true;

    protected override void Awake()
    {
        base.Awake();
        charController = GetComponent<CharacterController>();
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
        charController.UnFreeze();

        cooldown.Reset();
        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
        isLaunchingProjectile = false;

        charController.ApplyBump(-bumpForce * inputDir);

        InstanciateProjectiles();
    }

    private void InstanciateProjectiles()
    {
        int remainingProjectile = nbProjectile;
        ConeProjectile coneProjectile;
        Vector2 projectilePosition;
        if (nbProjectile.IsOdd())
        {
            projectilePosition = (Vector2)transform.position + (inputDir * instanciateDistance);
            coneProjectile = Instantiate(projectilePrefabs, projectilePosition, Quaternion.identity, CloneParent.cloneParent);
            coneProjectile.Launch(projectileSpeed, inputDir, this);
            nbProjectile--;
        }

        if (remainingProjectile <= 0)
            return;

        float currentAngle = Useful.WrapAngle(Useful.AngleHori(Vector2.zero, inputDir) - (coneAngle * 0.5f * Mathf.Deg2Rad));
        float angleStep = coneAngle / remainingProjectile;
        for (int i = 0; i < remainingProjectile; i++)
        {
            Vector2 dir = Useful.Vector2FromAngle(currentAngle);
            projectilePosition = (Vector2)transform.position + (instanciateDistance * dir);
            coneProjectile = Instantiate(projectilePrefabs, projectilePosition, Quaternion.identity, CloneParent.cloneParent);
            coneProjectile.Launch(projectileSpeed, (projectilePosition - (Vector2)transform.position).normalized, this);
            currentAngle = Useful.WrapAngle(currentAngle + angleStep);
        }
    }

    #region Gizmos/OnValidate

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        nbProjectile = Mathf.Max(nbProjectile, 1);
        bumpForce = Mathf.Max(bumpForce, 0f);
        projectileSpeed = Mathf.Max(projectileSpeed, 0f);
        castDuration = Mathf.Max(castDuration, 0f);
        instanciateDistance = Mathf.Max(instanciateDistance, 0f);
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
