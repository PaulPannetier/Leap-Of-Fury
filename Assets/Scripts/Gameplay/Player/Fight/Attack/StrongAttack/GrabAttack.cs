using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;
using System.Linq;

public class GrabAttack : StrongAttack
{
    private GameObject charTouch, walltouch;
    private CharacterController charController;
    private LayerMask charAndGroundMask, charMask;
    private Vector2 collisionPoint;
    private new Transform transform;
    private List<uint> charAlreadyTouch;
    private float currentWallGap;
    private bool isCharTouch => charTouch != null;

#if UNITY_EDITOR

    [SerializeField] private bool drawGizmos = true;

#endif

    [SerializeField] private float castRadius = 0.5f;
    [SerializeField] private float minRange = 1.5f;
    [SerializeField] private float range = 10f;
    [SerializeField] private float charCollisionRadius = 1f;
    [SerializeField] private Vector2 collisionOffset= Vector2.zero;
    [SerializeField] private bool keepSpeedAtEnd = true;

    [Header("Wall")]
    [SerializeField] private float waitingTimeWhenWallGrab = 0.1f;
    [SerializeField] private float maxWallGrabDuration = 0.7f;
    [SerializeField, Tooltip("The duration of the dash compare to the distance, in %age of maxWallGrabDuration")] private AnimationCurve wallDurationOverDistance;
    [SerializeField, Tooltip("The position (position progress) over time in %age")] private AnimationCurve wallPositionOverTime;
    [SerializeField] private float wallGapUp = 0.7f, wallGapDown, wallGapRight, wallGapLeft;

    [Header("Char")]
    [SerializeField] private StunEffectParams stunEffectParams = new StunEffectParams();
    [SerializeField] private float waitingTimeWhenCharGrab = 0.1f;
    [SerializeField] private float maxCharGrabDuration = 0.7f;
    [SerializeField, Tooltip("The duration of the dash compare to the distance, in %age of maxCharGrabDuration")] private AnimationCurve charDurationOverDistance;
    [SerializeField, Tooltip("The position (position progress) over time in %age")] private AnimationCurve charPositionOverTime;

    #region Awake/Start

    protected override void Awake()
    {
        base.Awake();
        this.transform = base.transform;
        charAlreadyTouch = new List<uint>();
    }

    protected override void Start()
    {
        base.Start();
        charController = GetComponent<CharacterController>();

        charAndGroundMask = LayerMask.GetMask("Char", "Floor", "WallProjectile");
        charMask = LayerMask.GetMask("Char");
    }

    #endregion

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

        Vector2 dir = charController.GetCurrentDirection(true);

        if (dir.x > Mathf.Epsilon)
        {
            if (dir.y > Mathf.Epsilon)
            {
                currentWallGap = (wallGapRight + wallGapUp) * 0.5f * Mathf.Sqrt(2f);
            }
            else if (dir.y < -Mathf.Epsilon)
            {
                currentWallGap = (wallGapRight + wallGapDown) * 0.5f * Mathf.Sqrt(2f);
            }
            else
            {
                currentWallGap = wallGapRight;
            }
        }
        else if (dir.x < -Mathf.Epsilon)
        {
            if (dir.y > Mathf.Epsilon)
            {
                currentWallGap = (wallGapLeft + wallGapUp) * 0.5f * Mathf.Sqrt(2f);
            }
            else if (dir.y < -Mathf.Epsilon)
            {
                currentWallGap = (wallGapLeft + wallGapDown) * 0.5f * Mathf.Sqrt(2f);
            }
            else
            {
                currentWallGap = wallGapLeft;
            }
        }
        else
        {
            if (dir.y > Mathf.Epsilon)
            {
                currentWallGap = wallGapUp;
            }
            else if (dir.y < -Mathf.Epsilon)
            {
                currentWallGap = wallGapDown;
            }
            else
            {
                currentWallGap = (wallGapRight + wallGapLeft) * 0.5f;
                Debug.LogWarning("Debug pls");
                LogManager.instance.WriteLog("Movement.GetCurrentDirection cannot return the vector : " + dir, dir, currentWallGap);
            }
        }

        ToricRaycastHit2D[] raycastAll = PhysicsToric.CircleCastAll(transform.position, dir, castRadius, range, charAndGroundMask);

        int RaycastComparer(ToricRaycastHit2D r1, ToricRaycastHit2D r2)
        {
            if (r1.distance < r2.distance)
                return -1;
            if (r1.distance > r2.distance)
                return 1;
            return 0;
        }

        Array.Sort(raycastAll, RaycastComparer);
        ToricRaycastHit2D raycast = default(ToricRaycastHit2D);
        bool isCharCollision = false;

        for (int i = 0; i < raycastAll.Length; i++)
        {
            if (raycastAll[i].collider.CompareTag("Char"))
            {
                GameObject player = raycastAll[i].collider.gameObject;
                if (player != gameObject)
                {
                    FightController charFc = player.GetComponent<FightController>();
                    if (raycastAll[i].distance > minRange && (int)charFc.damageProtectionType <= 1)
                    {
                        raycast = raycastAll[i];
                        isCharCollision = true;
                        break;
                    }
                }
            }
            else
            {
                if (raycastAll[i].distance > minRange)
                {
                    raycast = raycastAll[i];
                    isCharCollision = false;
                    break;
                }
            }
        }

        if (raycast.collider == null)
            return false;

        if (isCharCollision)
        {
            SetCharData(raycast);
        }
        else
        {
            SetWallData(raycast);
        }

        return true;

        void SetCharData(in ToricRaycastHit2D raycastChar)
        {
            charTouch = raycastChar.collider.gameObject;
            walltouch = null;
            collisionPoint = raycastChar.point;
        }

        void SetWallData(in ToricRaycastHit2D raycastGround)
        {
            walltouch = raycastGround.collider.gameObject;
            charTouch = null;
            collisionPoint = raycastGround.point;
        }
    }

    #endregion

    private IEnumerator PerformAttack(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        charAlreadyTouch.Clear();

        if(isCharTouch)
            yield return PerformCharTouch(callbackEnableOtherAttack, callbackEnableThisAttack);
        else
            yield return PerformWallTouch(callbackEnableOtherAttack, callbackEnableThisAttack);

        charController.UnFreeze();
        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();

        IEnumerator PerformCharTouch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
        {
            ApplyEffect(charTouch, effectType, new StunEffectParams(waitingTimeWhenCharGrab * 2f));
            charController.Freeze();

            yield return PauseManager.instance.Wait(waitingTimeWhenCharGrab);

            Vector2 begPos = transform.position;
            Vector2 oldPos = begPos;
            Vector2 dir = PhysicsToric.Direction(begPos, collisionPoint);

            float totalDistance = PhysicsToric.Distance(begPos, collisionPoint);
            float duration = maxCharGrabDuration * charDurationOverDistance.Evaluate(Mathf.Clamp01(totalDistance / range));

            float timeCounter = 0f;
            uint[] ignoreID = new uint[1] { charTouch.GetComponent<PlayerCommon>().id };

            while (timeCounter < duration)
            {
                PerformCollision(ignoreID);
                yield return null;
                timeCounter += Time.deltaTime;

                Vector2 newPos = begPos + (totalDistance * charPositionOverTime.Evaluate(Mathf.Clamp01(timeCounter / duration)) * dir);
                newPos = PhysicsToric.GetPointInsideBounds(newPos);
                oldPos = transform.position;
                charController.Teleport(newPos);

                while (PauseManager.instance.isPauseEnable)
                {
                    yield return null;
                }
            }

            if(keepSpeedAtEnd)
            {
                charController.ForceApplyVelocity(((Vector2)transform.position - oldPos) / Time.deltaTime);
            }

            OnTouchEnemy(charTouch.GetComponent<ToricObject>().original, damageType);
        }

        IEnumerator PerformWallTouch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
        {
            charController.Freeze();
            yield return PauseManager.instance.Wait(waitingTimeWhenWallGrab);

            Vector2 begPos = transform.position;
            Vector2 oldPos = begPos;
            Vector2 dir = PhysicsToric.Direction(begPos, collisionPoint);            

            Vector2 target = collisionPoint - currentWallGap * dir;
            float totalDistance = PhysicsToric.Distance(begPos, target);
            float duration = maxWallGrabDuration * wallDurationOverDistance.Evaluate(Mathf.Clamp01(totalDistance / range));

            float timeCounter = 0f;
            while(timeCounter < duration)
            {
                PerformCollision();
                yield return null;
                timeCounter += Time.deltaTime;

                Vector2 newPos = begPos + (totalDistance * wallPositionOverTime.Evaluate(Mathf.Clamp01(timeCounter / duration)) * dir);
                newPos = PhysicsToric.GetPointInsideBounds(newPos);
                oldPos = transform.position;
                charController.Teleport(newPos);

                while (PauseManager.instance.isPauseEnable)
                {
                    yield return null;
                }
            }

            if (keepSpeedAtEnd)
            {
                charController.ForceApplyVelocity(((Vector2)transform.position - oldPos) / Time.deltaTime);
            }
        }
    }

    private void PerformCollision(uint[] ignoreID = null)
    {
        Collider2D[] cols = PhysicsToric.OverlapCircleAll((Vector2)transform.position + collisionOffset, charCollisionRadius, charMask);

        foreach (Collider2D col in cols)
        {
            if (col.CompareTag("Char") && col.gameObject != gameObject)
            {
                PlayerCommon pc = col.GetComponent<PlayerCommon>();
                if (!charAlreadyTouch.Contains(pc.id) && (ignoreID == null || !ignoreID.Contains(pc.id)))
                {
                    charAlreadyTouch.Add(pc.id);
                    base.OnTouchEnemy(col.gameObject, damageType);
                }
            }
        }
    }

    #region Gizmos/OnValidate

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Gizmos.color = Color.green;
        Circle.GizmosDraw(transform.position, range);
        Circle.GizmosDraw(transform.position, minRange);
        Circle.GizmosDraw((Vector2)transform.position + range * Vector2.up, castRadius);
        Circle.GizmosDraw((Vector2)transform.position + collisionOffset, charCollisionRadius);
        Gizmos.color = Color.red;
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        this.transform = base.transform;
        stunEffectParams.OnValidate();
        range = Mathf.Max(0f, range);
        castRadius = Mathf.Max(0f, castRadius);
        waitingTimeWhenWallGrab = Mathf.Max(0f, waitingTimeWhenWallGrab);
        waitingTimeWhenCharGrab = Mathf.Max(0f, waitingTimeWhenCharGrab);
        maxWallGrabDuration = Mathf.Max(0f, maxWallGrabDuration);
        maxCharGrabDuration = Mathf.Max(0f, maxCharGrabDuration);
        charCollisionRadius = Mathf.Max(0f, charCollisionRadius);
        wallGapUp = Mathf.Max(0f, wallGapUp);
        wallGapDown = Mathf.Max(0f, wallGapDown);
        wallGapLeft = Mathf.Max(0f, wallGapLeft);
        wallGapRight = Mathf.Max(0f, wallGapRight);
        charAndGroundMask = LayerMask.GetMask("Char", "Floor", "WallProjectile");
        charMask = LayerMask.GetMask("Char");
    }

#endif

    #endregion
}
