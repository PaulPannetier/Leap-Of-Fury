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
    private Movement movement;
    private LayerMask charMask, groundMask;
    private Vector2 collisionPoint;
    private new Transform transform;
    private List<uint> charAlreadyTouch;

    private bool isCharTouch => charTouch != null;

    [SerializeField] private float castRadius = 0.5f;
    [SerializeField] private float range = 10f;
    [SerializeField] private float collisionRadius = 1f;
    [SerializeField] private Vector2 collisionOffset= Vector2.zero;
    [SerializeField] private bool keepSpeedAtEnd = true;

    [Header("Wall")]
    [SerializeField] private float waitingTimeWhenWallGrab = 0.1f;
    [SerializeField] private float maxWallGrabDuration = 0.7f;
    [SerializeField, Tooltip("The duration of the dash compare to the distance, in %age of maxWallGrabDuration")] private AnimationCurve wallDurationOverDistance;
    [SerializeField, Tooltip("The position (position progress) over time in %age")] private AnimationCurve wallPositionOverTime;
    [SerializeField] private float wallGap = 0.5f;

    [Header("Char")]
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
        movement = GetComponent<Movement>();

        charMask = LayerMask.GetMask("Char");
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
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

        Vector2 dir = movement.GetCurrentDirection(true);
        RaycastHit2D[] raycastCharAll = PhysicsToric.CircleCastAll(transform.position, dir, castRadius, range, charMask);
        RaycastHit2D raycastChar = default(RaycastHit2D);

        float minDist = float.MaxValue;
        for (int i = 0; i < raycastCharAll.Length; i++)
        {
            GameObject player = raycastCharAll[i].collider.gameObject;
            if (player != gameObject)
            {
                FightController charFc = player.GetComponent<FightController>();
                if (charFc.canBeStun && raycastCharAll[i].distance < minDist)
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
        else //raycastGround.collider != null
        {
            SetWallData(raycastGround);
        }

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
        charAlreadyTouch.Clear();

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
            print("char");
            FightController charFc = charTouch.GetComponent<FightController>();
            charFc.RequestStun(-1);
            movement.Freeze();

            yield return Wait(waitingTimeWhenCharGrab);

            float duration = maxCharGrabDuration * charDurationOverDistance.Evaluate(Mathf.Clamp01(((Vector2)transform.position).Distance(collisionPoint) / range));
            Vector2 begPos = transform.position;
            float timeCounter = 0f;
            uint[] ignoreID = new uint[1] { charTouch.GetComponent<PlayerCommon>().id };


            while (timeCounter < duration)
            {
                PerformCollision(ignoreID);
                yield return null;
                timeCounter += Time.deltaTime;

                Vector2 newPos = Vector2.Lerp(begPos, collisionPoint, charPositionOverTime.Evaluate(timeCounter / duration));
                movement.Teleport(newPos);

                while (PauseManager.instance.isPauseEnable)
                {
                    yield return null;
                }
            }

            movement.UnFreeze();

            OnTouchEnemy(charTouch);

            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
        }

        IEnumerator PerformWallTouch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
        {
            print("Wall");

            movement.Freeze();
            yield return Wait(waitingTimeWhenWallGrab);

            Vector2 dir = (collisionPoint - (Vector2)transform.position).normalized;
            Vector2 target = collisionPoint - wallGap * dir;
            float duration = maxWallGrabDuration * wallDurationOverDistance.Evaluate(Mathf.Clamp01(((Vector2)transform.position).Distance(target) / range));

            Vector2 begPos = transform.position;

            float timeCounter = 0f;
            while(timeCounter < duration)
            {
                PerformCollision();
                yield return null;
                timeCounter += Time.deltaTime;

                Vector2 newPos = Vector2.Lerp(begPos, target, wallPositionOverTime.Evaluate(timeCounter / duration));
                movement.Teleport(newPos);

                while (PauseManager.instance.isPauseEnable)
                {
                    yield return null;
                }
            }

            movement.UnFreeze();

            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
        }
    }

    private void PerformCollision(uint[] ignoreID = null)
    {
        Collider2D[] cols = PhysicsToric.OverlapCircleAll((Vector2)transform.position + collisionOffset, collisionRadius, charMask);

        foreach (Collider2D col in cols)
        {
            if (col.CompareTag("Char") && col.gameObject != gameObject)
            {
                PlayerCommon pc = col.GetComponent<PlayerCommon>();
                if (!charAlreadyTouch.Contains(pc.id) && (ignoreID != null || !ignoreID.Contains(pc.id)))
                {
                    charAlreadyTouch.Add(pc.id);
                    base.OnTouchEnemy(col.gameObject);
                }
            }
        }
    }

    #region Gizmos/OnValidate

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw(transform.position, range);
        Circle.GizmosDraw((Vector2)transform.position + range * Vector2.up, castRadius);
        Circle.GizmosDraw((Vector2)transform.position + collisionOffset, collisionRadius);
        Gizmos.color = Color.red;
        Circle.GizmosDraw((Vector2)transform.position + range * Vector2.up, wallGap);
    }

    private void OnValidate()
    {
        range = Mathf.Max(0f, range);
        castRadius = Mathf.Max(0f, castRadius);
        waitingTimeWhenWallGrab = Mathf.Max(0f, waitingTimeWhenWallGrab);
        waitingTimeWhenCharGrab = Mathf.Max(0f, waitingTimeWhenCharGrab);
        maxWallGrabDuration = Mathf.Max(0f, maxWallGrabDuration);
        collisionRadius = Mathf.Max(0f, collisionRadius);
        wallGap = Mathf.Max(0f, wallGap);
        this.transform = base.transform;
    }

#endif

    #endregion
}
