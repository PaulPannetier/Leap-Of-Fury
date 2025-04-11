using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;

public class GrabAttack : StrongAttack
{
    private CharacterController charController;
    private LayerMask groundMask, charMask;
    private new Transform transform;
    private List<uint> charAlreadyTouch;

#if UNITY_EDITOR
    [SerializeField] private bool drawGizmos = true;
#endif

    [Header("General Settings")]
    [SerializeField, Range(0f, 180f)] private float castAngle;
    [SerializeField] private float grabRange;
    [SerializeField] private byte nbRaycast = 10;
    [SerializeField] private float minGrabDistance;
    [SerializeField] private bool only8Dir = true;
    [SerializeField] private Vector2 killOffset;
    [SerializeField] private float killRadius;
    [SerializeField] private bool keepSpeedAtEnd = true;

    [Header("Wall")]
    [SerializeField] private float waitingTimeWhenWallGrab = 0.1f;
    [SerializeField] private float maxWallGrabDuration = 0.7f;
    [SerializeField, Tooltip("The duration of the dash compare to the distance, in %age of maxWallGrabDuration")] private AnimationCurve wallDurationOverDistance;
    [SerializeField, Tooltip("The position (position progress) over time in %age")] private AnimationCurve wallPositionOverTime;
    [SerializeField] private float wallGapUp = 0.7f, wallGapDown, wallGapRight, wallGapLeft;
    [SerializeField] private LineRenderer wallRendererPrefabs;

    [Header("Char")]
    [SerializeField] private StunEffectParams stunEffectParams = new StunEffectParams();
    [SerializeField] private float waitingTimeWhenCharGrab = 0.1f;
    [SerializeField] private float maxCharGrabDuration = 0.7f;
    [SerializeField, Tooltip("The duration of the dash compare to the distance, in %age of maxCharGrabDuration")] private AnimationCurve charDurationOverDistance;
    [SerializeField, Tooltip("The position (position progress) over time in %age")] private AnimationCurve charPositionOverTime;
    [SerializeField] private LineRenderer charRendererPrefabs;

    #region Awake/Start

    protected override void Awake()
    {
        base.Awake();
        this.transform = base.transform;
        charController = GetComponent<CharacterController>();
        charAlreadyTouch = new List<uint>(4);
        if (nbRaycast.IsEven())
            nbRaycast++;
    }

    protected override void Start()
    {
        base.Start();
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
        charMask = LayerMask.GetMask("Char");
    }

    #endregion

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if (!cooldown.isActive)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        Vector2 dir = charController.GetCurrentDirection(only8Dir);
        Tuple<ToricRaycastHit2D, ToricRaycastHit2D> raycasts = PerformRaycasts(dir);
        ToricRaycastHit2D? finalRay = GetPreferedRaycast(raycasts.Item1, raycasts.Item2);

        if (!finalRay.HasValue)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        base.Launch(callbackEnableOtherAttack, callbackEnableThisAttack);
        cooldown.Reset();

        StartCoroutine(PerformAttack(finalRay.Value, callbackEnableOtherAttack, callbackEnableThisAttack));

        return true;
    }

    #region Raycast

    private ToricRaycastHit2D? GetPreferedRaycast(in ToricRaycastHit2D playerRaycast, in ToricRaycastHit2D groundRaycast)
    {
        ToricRaycastHit2D? finalRay = playerRaycast.collider == null ? null : playerRaycast;
        if (finalRay.HasValue)
        {
            if (groundRaycast.collider != null && groundRaycast.distance < finalRay.Value.distance)
                finalRay = groundRaycast;
        }
        else
        {
            if (groundRaycast.collider != null)
                finalRay = groundRaycast;
        }
        return finalRay;
    }

    private Tuple<ToricRaycastHit2D, ToricRaycastHit2D> PerformRaycasts(in Vector2 dir)
    {
        ToricRaycastHit2D? playerRay = null;
        float playerRayDistance = float.MaxValue;
        ToricRaycastHit2D? groundRay = null;
        float groundRayDistance = float.MaxValue;

        float currentAngle = Useful.WrapAngle(Useful.AngleHori(Vector2.zero, dir) - (0.5f * castAngle * Mathf.Deg2Rad));
        float angleStep = castAngle * Mathf.Deg2Rad / (nbRaycast - 1);
        Vector2 start = transform.position;
        for (int i = 0; i < nbRaycast; i++)
        {
            Vector2 currentDir = Useful.Vector2FromAngle(currentAngle);
            ToricRaycastHit2D ground = PhysicsToric.Raycast(start, currentDir, grabRange, groundMask);
            if(ground.collider != null)
            {
                if(ground.distance < groundRayDistance)
                {
                    groundRay = ground;
                    groundRayDistance = ground.distance;
                }
            }

            ToricRaycastHit2D[] playerRaycasts = PhysicsToric.RaycastAll(start, currentDir, grabRange, charMask);
            ToricRaycastHit2D? closestPlayerRaycast = null;
            float closestDistance = float.MinValue;

            for (int j = 0; j < playerRaycasts.Length; j++)
            {
                ToricRaycastHit2D current = playerRaycasts[j];
                if (current.collider != null && current.collider.CompareTag("Char"))
                {
                    uint id = current.collider.GetComponent<ToricObject>().original.GetComponent<PlayerCommon>().id;
                    if(id != playerCommon.id)
                    {
                        if(closestDistance > current.distance)
                        {
                            closestDistance = current.distance;
                            closestPlayerRaycast = current;
                        }
                    }
                }
            }

            if(closestPlayerRaycast.HasValue && closestPlayerRaycast.Value.distance < playerRayDistance)
            {
                playerRayDistance = closestPlayerRaycast.Value.distance;
                playerRay = closestPlayerRaycast.Value;
            }

            currentAngle = Useful.WrapAngle(currentAngle + angleStep);
        }

        return new Tuple<ToricRaycastHit2D, ToricRaycastHit2D>(playerRay.HasValue ? playerRay.Value : default(ToricRaycastHit2D), groundRay.HasValue ? groundRay.Value : default(ToricRaycastHit2D));
    }

    #endregion

    private float GetWallCap(in Vector3 dir)
    {
        float currentWallGap;
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
            }
        }
        return currentWallGap;
    }

    private void PerformCharCollision(uint? ignoreId)
    {
        Collider2D[] cols = PhysicsToric.OverlapCircleAll((Vector2)transform.position + killOffset, killRadius, charMask);
        foreach(Collider2D col in cols)
        {
            if(col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                uint id = player.GetComponent<PlayerCommon>().id;
                if(id != playerCommon.id && (!ignoreId.HasValue || (ignoreId.Value != id)) && !charAlreadyTouch.Contains(id))
                {
                    OnTouchEnemy(player, damageType);
                    charAlreadyTouch.Add(id);
                }
            }
        }
    }

    private IEnumerator PerformAttack(ToricRaycastHit2D raycast, Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        charAlreadyTouch.Clear();
        bool isCharTouch = raycast.collider.CompareTag("Char");
        if (isCharTouch)
            yield return PerformCharTouch(raycast, callbackEnableOtherAttack, callbackEnableThisAttack);
        else
            yield return PerformWallTouch(raycast, callbackEnableOtherAttack, callbackEnableThisAttack);

        charController.UnFreeze();
        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();

        IEnumerator PerformCharTouch(ToricRaycastHit2D raycast, Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
        {
            GameObject playerTouch = raycast.collider.GetComponent<ToricObject>().original;
            ApplyEffect(playerTouch, effectType, new StunEffectParams(waitingTimeWhenCharGrab * 2f));
            charController.Freeze();

            yield return PauseManager.instance.Wait(waitingTimeWhenCharGrab);

            Vector2 start = transform.position;
            Vector2 oldPos = start;
            Vector2 dir;
            float totalDistance;
            (dir, totalDistance) = PhysicsToric.DirectionAndDistance(start, raycast.point);
            float duration = maxCharGrabDuration * charDurationOverDistance.Evaluate(Mathf.Clamp01(totalDistance / grabRange));

            float timeCounter = 0f;
            uint? ignoreID = playerTouch.GetComponent<PlayerCommon>().id;

            LineRenderer lineRenderer = Instantiate(charRendererPrefabs, Vector3.zero, Quaternion.identity, CloneParent.cloneParent);
            Vector3[] lineRendererPositions = new Vector3[] { start, playerTouch.transform.position };
            lineRenderer.SetPositions(lineRendererPositions);

            while (timeCounter < duration)
            {
                PerformCharCollision(ignoreID);
                yield return null;
                timeCounter += Time.deltaTime;

                Vector2 newPos = start + (totalDistance * charPositionOverTime.Evaluate(Mathf.Clamp01(timeCounter / duration)) * dir);
                newPos = PhysicsToric.GetPointInsideBounds(newPos);
                oldPos = transform.position;
                charController.Teleport(newPos);

                lineRendererPositions[0] = newPos;
                lineRendererPositions[1] = playerTouch.transform.position;
                lineRenderer.ResetPositions(lineRendererPositions);
                while (PauseManager.instance.isPauseEnable)
                {
                    yield return null;
                }
            }

            if(keepSpeedAtEnd)
            {
                charController.ForceApplyVelocity(((Vector2)transform.position - oldPos) / Time.deltaTime);
            }

            OnTouchEnemy(playerTouch, damageType);
            lineRenderer.ResetPositions(Array.Empty<Vector3>());
            Destroy(lineRenderer);
        }

        IEnumerator PerformWallTouch(ToricRaycastHit2D raycast, Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
        {
            charController.Freeze();
            yield return PauseManager.instance.Wait(waitingTimeWhenWallGrab);

            Vector2 start = transform.position;
            Vector2 oldPos = start;
            Vector2 dir = PhysicsToric.Direction(start, raycast.point);
            float wallGap = GetWallCap(dir);
            Vector2 target = raycast.point - (wallGap * dir);
            float totalDistance = PhysicsToric.Distance(start, target);
            float duration = maxWallGrabDuration * wallDurationOverDistance.Evaluate(Mathf.Clamp01(totalDistance / grabRange));

            float timeCounter = 0f;
            while(timeCounter < duration)
            {
                PerformCharCollision(null);
                yield return null;
                timeCounter += Time.deltaTime;

                Vector2 newPos = start + (totalDistance * wallPositionOverTime.Evaluate(Mathf.Clamp01(timeCounter / duration)) * dir);
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

    #region Gizmos/OnValidate

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Circle.GizmosDraw(transform.position, grabRange, Color.green);
        Circle.GizmosDraw(transform.position, minGrabDistance, Color.green);

        Circle.GizmosDraw((Vector2)transform.position + killOffset, killRadius, Color.black);

        //Raycast
        Vector2 dir = Vector2.right;
        int nbRaycast = this.nbRaycast.IsEven() ? this.nbRaycast + 1 : this.nbRaycast;

        float currentAngle = Useful.WrapAngle(Useful.AngleHori(Vector2.zero, dir) - (0.5f * castAngle * Mathf.Deg2Rad));
        float angleStep = castAngle * Mathf.Deg2Rad / (nbRaycast - 1);
        Vector2 start = transform.position;
        for (int i = 0; i < nbRaycast; i++)
        {
            Vector2 currentDir = Useful.Vector2FromAngle(currentAngle);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(start, start + (grabRange * currentDir));
            currentAngle = Useful.WrapAngle(currentAngle + angleStep);
        }
    }

    private void Reset()
    {
        effectType = FightController.EffectType.Stun;
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        charMask = LayerMask.GetMask("Char");
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
        this.transform = base.transform;
        stunEffectParams.OnValidate();
        grabRange = Mathf.Max(0f, grabRange);
        nbRaycast = (byte)Mathf.Max(0, nbRaycast);
        minGrabDistance = Mathf.Max(0f, minGrabDistance);
        nbRaycast = (byte)Mathf.Max(0, nbRaycast);
        waitingTimeWhenWallGrab = Mathf.Max(0f, waitingTimeWhenWallGrab);
        waitingTimeWhenCharGrab = Mathf.Max(0f, waitingTimeWhenCharGrab);
        maxWallGrabDuration = Mathf.Max(0f, maxWallGrabDuration);
        maxCharGrabDuration = Mathf.Max(0f, maxCharGrabDuration);
        wallGapUp = Mathf.Max(0f, wallGapUp);
        wallGapDown = Mathf.Max(0f, wallGapDown);
        wallGapLeft = Mathf.Max(0f, wallGapLeft);
        wallGapRight = Mathf.Max(0f, wallGapRight);
        killRadius = Mathf.Max(0f, killRadius);
        effectType = FightController.EffectType.Stun;
    }

#endif

    #endregion
}
