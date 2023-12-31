using System;
using System.Collections;
using UnityEngine;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;
using System.Linq;

public class GrabAttack : StrongAttack
{
    private GameObject charTouch, walltouch;
    private Movement movement;
    private LayerMask charMask, groundMask;
    private Vector2 collisionPoint;

    private bool isCharTouch => charTouch != null;

    [SerializeField] private float castRadius = 0.5f;
    [SerializeField] private float range = 10f;

    [Header("Wall")]
    [SerializeField] private float waitingTimeWhenWallGrab = 0.1f;

    [Header("Char")]
    [SerializeField] private float waitingTimeWhenCharGrab = 0.1f;

    protected override void Start()
    {
        base.Start();
        movement = GetComponent<Movement>();

        charMask = LayerMask.GetMask("Char");
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
    }

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if (!cooldown.isActive || !PerformSpherecast())
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        base.Launch(callbackEnableOtherAttack, callbackEnableThisAttack);
        cooldown.Reset();

        StartCoroutine(PerformAttack(callbackEnableOtherAttack, callbackEnableThisAttack));

        return true;
    }

    #region SphereCast

    private bool PerformSpherecast()
    {
        charTouch = walltouch = null;

        Vector2 dir = movement.GetCurrentDirection(true);
        RaycastHit2D[] raycastCharAll = PhysicsToric.CircleCastAll(transform.position, dir, castRadius, range, charMask);
        RaycastHit2D raycastChar = default(RaycastHit2D);

        float minDist = float.MaxValue;
        for (int i = 0; i < raycastCharAll.Length; i++)
        {
            if (raycastCharAll[i].collider.gameObject != gameObject)
            {
                if (raycastCharAll[i].distance < minDist)
                {
                    raycastChar = raycastCharAll[i];
                    minDist = raycastChar.distance;
                }
            }
        }

        RaycastHit2D raycastGround = PhysicsToric.CircleCast(transform.position, dir, castRadius, range, groundMask);

        if(raycastChar.collider == null && raycastGround.collider == null)
            return false;

        if (raycastChar.collider != null && raycastGround.collider != null)
        {
            if(raycastChar.distance <= raycastGround.distance)
                SetCharData(raycastChar);
            else
                SetWallData(raycastGround);
        }

        if (raycastChar.collider != null)
        {
            SetCharData(raycastChar);
        }

        //raycastGround.collider != null
        SetWallData(raycastGround);

        return true;

        void SetCharData(in RaycastHit2D raycastChar)
        {
            charTouch = raycastChar.collider.gameObject;
            collisionPoint = raycastChar.point;
        }

        void SetWallData(in RaycastHit2D raycastGround)
        {
            walltouch = raycastGround.collider.gameObject;
            collisionPoint = raycastGround.point;
        }
    }

    #endregion

    private IEnumerator PerformAttack(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if(isCharTouch)
            yield return PerformCharTouch(callbackEnableOtherAttack, callbackEnableThisAttack);
        else
            yield return PerformWallTouch(callbackEnableOtherAttack, callbackEnableThisAttack);

        IEnumerator Wait(float duration)
        {
            float counter = 0f;
            while (counter < duration)
            {
                yield return null;
                if (!PauseManager.instance.isPauseEnable)
                {
                    counter += Time.deltaTime;
                }
            }
        }

        IEnumerator PerformCharTouch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
        {
            yield return Wait(waitingTimeWhenCharGrab);

            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
        }

        IEnumerator PerformWallTouch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
        {
            yield return Wait(waitingTimeWhenWallGrab);


            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
        }
    }

    #region Gizmos/OnValidate

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw(transform.position, range);
        Circle.GizmosDraw((Vector2)transform.position + range * Vector2.up, castRadius);
    }

    private void OnValidate()
    {
        range = Mathf.Max(0f, range);
        castRadius = Mathf.Max(0f, castRadius);
        waitingTimeWhenWallGrab = Mathf.Max(0f, waitingTimeWhenWallGrab);
        waitingTimeWhenCharGrab = Mathf.Max(0f, waitingTimeWhenCharGrab);
    }

#endif

    #endregion
}
