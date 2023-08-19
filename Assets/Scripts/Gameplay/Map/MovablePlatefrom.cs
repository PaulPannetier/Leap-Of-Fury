using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class MovablePlatefrom : MonoBehaviour
{
    private enum HitboxSide
    {
        up = 0, down = 1, left = 2, right = 3, none = 4
    }

    private static Vector2[] convertHitboxSideToDir = new Vector2[5]
    {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right, Vector2.zero
    };

    private BoxCollider2D hitbox;
    private new Transform transform;
    private LayerMask charMask, groundMask;
    private bool isMoving, isTargetingPosition;
    private Rigidbody2D rb;
    private GameObject lastCharActivatePlateform;
    private uint lastCharIdActivate;
    private Vector2[] convertHitboxSideToBumbDir;
    private float lastTimeActivated = -10f;
    private HitboxSide lastSideActiavated = HitboxSide.none;
    private Vector2 targetPosition;
    private List<uint> charAlreadyCrush;
    private PauseData pauseData;
    private bool pauseWasEnableLastFrame;

    public bool enableBehaviour = true;
    [SerializeField] private bool enableLeftAndRightDash, enableUpAndDownDash;
    [SerializeField, Tooltip("The width or height on the detection hitbox when moving")] private float collisionOverlapSize = 1f;
    [SerializeField, Tooltip("La marge d'erreur de détection de crush")] private float crushPadding = 1.05f;
    [SerializeField, Tooltip("The dash detection hitbox")] private Vector2 dashHitbox;
    [SerializeField] private float activationCooldown = 0.3f;
    [SerializeField] private float accelerationDuration = 1f;
    [SerializeField, Tooltip("In %age of maxSpeed")] private AnimationCurve accelerationCurve;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float returnBumpSpeedOnChar = 5f;
    [SerializeField, Range(0f, 360f)] private float returnBumpSpeedAngleOnChar = 45f;

#if UNITY_EDITOR

    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private bool drawGroundDetectionHitoxGizmos = true, drawCrushHiboxGizmos = true, drawDashHiboxGizmos = true;
    [SerializeField] private Color colorGroundDetectionHitbox, colorCrushDetectionHitbox, colorDashDetectionHitbox;

#endif

    #region Awake and Start

    private void Awake()
    {
        hitbox = GetComponent<BoxCollider2D>();
        transform = base.transform;
        rb = GetComponent<Rigidbody2D>();
        convertHitboxSideToBumbDir = new Vector2[5]
        {
            Vector2.up,
            Vector2.down,
            Useful.Vector2FromAngle(returnBumpSpeedAngleOnChar * Mathf.Deg2Rad),
            Useful.Vector2FromAngle((180f - returnBumpSpeedAngleOnChar) * Mathf.Deg2Rad),
            Vector2.zero
        };
        charAlreadyCrush = new List<uint>();
    }

    private void Start()
    {
        PauseManager.instance.callBackOnPauseDisable += Enable;
        PauseManager.instance.callBackOnPauseEnable += Disable;
        charMask = LayerMask.GetMask("Char");
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
    }

    #endregion

    #region FixedUpdate

    private void FixedUpdate()
    {
        if(!enableBehaviour)
            return;

        if(pauseWasEnableLastFrame)
        {
            lastTimeActivated = Time.time - pauseData.lastTimeBeginMoveDeltaTime;
            pauseWasEnableLastFrame = false;
        }
        
        if(isMoving)
        {
            HandleMoving();
        }
        else
        {
            HandleStay();
        }

        void HandleMoving()
        {
            //ground Detection
            if (!isTargetingPosition && IsGroundCollision(lastSideActiavated, out Collider2D groundCol))
            {
                targetPosition = CalculateTargetPos(lastSideActiavated, groundCol);
                isTargetingPosition = true;
            }



            //speed
            if (isTargetingPosition)
            {
                rb.velocity = maxSpeed * convertHitboxSideToDir[(int)lastSideActiavated];
                if(((Vector2)transform.position).SqrDistance(targetPosition) < 4f * Time.fixedDeltaTime * Time.fixedDeltaTime * rb.velocity.sqrMagnitude)
                {
                    OnStopOnGround();
                }
            }
            else
            {
                float speed = Time.time - lastTimeActivated > accelerationDuration ? maxSpeed : maxSpeed * accelerationCurve.Evaluate((Time.time - lastTimeActivated) / accelerationDuration);
                rb.velocity = speed * convertHitboxSideToDir[(int)lastSideActiavated];
            }

            //char dash detecttion
            if(!isTargetingPosition)
            {
                Collider2D[] cols;
                if (enableLeftAndRightDash)
                {
                    //up
                    cols = GetCharColliders(HitboxSide.up);
                    HandleDashCollision(cols, HitboxSide.up);
                    //down
                    cols = GetCharColliders(HitboxSide.down);
                    HandleDashCollision(cols, HitboxSide.down);
                }

                if (enableLeftAndRightDash)
                {
                    //right
                    cols = GetCharColliders(HitboxSide.right);
                    HandleDashCollision(cols, HitboxSide.right);
                    //left
                    cols = GetCharColliders(HitboxSide.left);
                    HandleDashCollision(cols, HitboxSide.left);
                }

                void HandleDashCollision(Collider2D[] cols, HitboxSide hitboxSide)
                {
                    if (lastSideActiavated == hitboxSide)
                        return;

                    foreach (Collider2D col in cols)
                    {
                        if (col.CompareTag("Char"))
                        {
                            GameObject player = col.GetComponent<ToricObject>().original;
                            if (IsPlayerDash(player))
                            {
                                if (lastCharIdActivate != player.GetComponent<PlayerCommon>().id)
                                {
                                    if (Time.time - lastTimeActivated > activationCooldown)
                                    {
                                        rb.velocity = Vector2.zero;
                                        OnActivated(player, hitboxSide);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Crushing Char
            if(false && CrushDetectionSimple(lastSideActiavated))
            {
                HandleCrushDetectionAdvance(lastSideActiavated);
            }

            bool CrushDetectionSimple(HitboxSide hitboxSide)
            {
                Vector2 center, size;
                switch (hitboxSide)
                {
                    case HitboxSide.up:
                        center = new Vector2(transform.position.x, transform.position.y + hitbox.size.y * 0.5f + PlayerCommon.charSize.y * crushPadding * 0.5f);
                        size = new Vector2(hitbox.size.x, PlayerCommon.charSize.y * crushPadding);
                        break;
                    case HitboxSide.down:
                        center = new Vector2(transform.position.x, transform.position.y - hitbox.size.y * 0.5f - PlayerCommon.charSize.y * crushPadding * 0.5f);
                        size = new Vector2(hitbox.size.x, PlayerCommon.charSize.y * crushPadding);
                        break;
                    case HitboxSide.left:
                        center = new Vector2(transform.position.x - hitbox.size.x * 0.5f - PlayerCommon.charSize.x * crushPadding * 0.5f, transform.position.y);
                        size = new Vector2(PlayerCommon.charSize.x * crushPadding, hitbox.size.y);
                        break;
                    case HitboxSide.right:
                        center = new Vector2(transform.position.x + hitbox.size.x * 0.5f + PlayerCommon.charSize.x * crushPadding * 0.5f, transform.position.y);
                        size = new Vector2(PlayerCommon.charSize.x * crushPadding, hitbox.size.y);
                        break;
                    default:
                        return false;
                }

                Collider2D[] groundCol = PhysicsToric.OverlapBoxAll(center, size, 0f, groundMask);
                bool isGroundCollision = false;
                foreach (Collider2D col in groundCol)
                {
                    if(col != hitbox)
                    {
                        isGroundCollision = true;
                        break;
                    }
                }
                if (!isGroundCollision)
                    return false;

                Collider2D charCols = PhysicsToric.OverlapBox(center, size, 0f, charMask);
                return charCols != null;
            }

            void HandleCrushDetectionAdvance(HitboxSide hitboxSide)
            {
                Vector2 begPoint, step, size;
                int nbStep;
                switch (hitboxSide)
                {
                    case HitboxSide.up:
                        begPoint = new Vector2(transform.position.x + (hitbox.size.x - LevelMapData.currentMap.cellSize.x) * 0.5f, transform.position.y + hitbox.size.y * 0.5f + PlayerCommon.charSize.y * crushPadding * 0.5f);
                        size = new Vector2(LevelMapData.currentMap.cellSize.x, PlayerCommon.charSize.y * crushPadding);
                        step = new Vector2(-LevelMapData.currentMap.cellSize.y, 0f);
                        nbStep = hitbox.size.x.Round();
                        break;
                    case HitboxSide.down:
                        begPoint = new Vector2(transform.position.x + (hitbox.size.x - LevelMapData.currentMap.cellSize.x) * 0.5f, transform.position.y - hitbox.size.y * 0.5f - PlayerCommon.charSize.y * crushPadding * 0.5f);
                        size = new Vector2(LevelMapData.currentMap.cellSize.x, PlayerCommon.charSize.y * crushPadding);
                        step = new Vector2(-LevelMapData.currentMap.cellSize.y, 0f);
                        nbStep = hitbox.size.x.Round();
                        break;
                    case HitboxSide.left:
                        begPoint = new Vector2(transform.position.x - hitbox.size.x * 0.5f - PlayerCommon.charSize.x * crushPadding * 0.5f, transform.position.y + (hitbox.size.y - LevelMapData.currentMap.cellSize.y) * 0.5f);
                        size = new Vector2(PlayerCommon.charSize.x * crushPadding, LevelMapData.currentMap.cellSize.y);
                        step = new Vector2(0f, -LevelMapData.currentMap.cellSize.y);
                        nbStep = hitbox.size.y.Round();
                        break;
                    case HitboxSide.right:
                        begPoint = new Vector2(transform.position.x + hitbox.size.x * 0.5f + PlayerCommon.charSize.x * crushPadding * 0.5f, transform.position.y + (hitbox.size.y - LevelMapData.currentMap.cellSize.y) * 0.5f);
                        size = new Vector2(PlayerCommon.charSize.x * crushPadding, LevelMapData.currentMap.cellSize.y);
                        step = new Vector2(0f, -LevelMapData.currentMap.cellSize.y);
                        nbStep = hitbox.size.y.Round();
                        break;
                    default:
                        return;
                }

                for (int i = 0; i < nbStep; i++)
                {
                    Collider2D[] groundCols = PhysicsToric.OverlapBoxAll(begPoint, size, 0f, groundMask);
                    bool isGroundCol = false;
                    foreach (Collider2D col in groundCols)
                    {
                        if(col != hitbox)
                        {
                            isGroundCol = true;
                            break;
                        }
                    }

                    if(isGroundCol)
                    {
                        Collider2D[] cols = PhysicsToric.OverlapBoxAll(begPoint, size, 0f, charMask);
                        foreach(Collider2D col in cols)
                        {
                            if(col.CompareTag("Char"))
                            {
                                GameObject player = col.GetComponent<ToricObject>().original;
                                if(!charAlreadyCrush.Contains(player.GetComponent<PlayerCommon>().id))
                                {
                                    OnCrushChar(player, lastCharActivatePlateform);
                                }
                            }
                        }
                    }
                }
            }
        }

        void HandleStay()
        {
            //detect char dash
            Collider2D[] cols;
            if (enableLeftAndRightDash)
            {
                //up
                cols = GetCharColliders(HitboxSide.up);
                HandleDashCollision(cols, HitboxSide.up);
                //down
                cols = GetCharColliders(HitboxSide.down);
                HandleDashCollision(cols, HitboxSide.down);
            }

            if (enableLeftAndRightDash)
            {
                //right
                cols = GetCharColliders(HitboxSide.right);
                HandleDashCollision(cols, HitboxSide.right);
                //left
                cols = GetCharColliders(HitboxSide.left);
                HandleDashCollision(cols, HitboxSide.left);
            }

            void HandleDashCollision(Collider2D[] cols, HitboxSide hitboxSide)
            {
                if (isMoving)
                    return;

                foreach (Collider2D col in cols)
                {
                    if (col.CompareTag("Char"))
                    {
                        GameObject player = col.GetComponent<ToricObject>().original;
                        if (IsPlayerDash(player))
                        {
                            if (Time.time - lastTimeActivated > activationCooldown)
                            {
                                OnActivated(player, hitboxSide);
                            }
                        }
                    }
                }
            }
        }

        bool IsPlayerDash(GameObject player)
        {
            return player.GetComponent<Movement>().isDashing;
        }

        Collider2D[] GetCharColliders(HitboxSide hitboxSide)
        {
            Vector2 center, size;
            switch (hitboxSide)
            {
                case HitboxSide.up:
                    center = new Vector2(transform.position.x, transform.position.y + dashHitbox.y * 0.25f);
                    size = new Vector2(hitbox.size.x, dashHitbox.y * 0.5f);
                    break;
                case HitboxSide.down:
                    center = new Vector2(transform.position.x, transform.position.y - dashHitbox.y * 0.25f);
                    size = new Vector2(hitbox.size.x, dashHitbox.y * 0.5f);
                    break;
                case HitboxSide.left:
                    center = new Vector2(transform.position.x - dashHitbox.x * 0.25f, transform.position.y);
                    size = new Vector2(dashHitbox.x * 0.5f, hitbox.size.y);
                    break;
                case HitboxSide.right:
                    center = new Vector2(transform.position.x + dashHitbox.x * 0.25f, transform.position.y);
                    size = new Vector2(dashHitbox.x * 0.5f, hitbox.size.y);
                    break;
                default:
                    return default(Collider2D[]);
            }
            return PhysicsToric.OverlapBoxAll(center, size, 0, charMask);
        }
    }

    bool IsGroundCollision(HitboxSide hitboxSide, out Collider2D groundCol)
    {
        Collider2D[] groundCols;
        switch (hitboxSide)
        {
            case HitboxSide.up:
                groundCols = PhysicsToric.OverlapBoxAll(new Vector2(transform.position.x, transform.position.y + (hitbox.size.y + collisionOverlapSize) * 0.5f), new Vector2(hitbox.size.x, collisionOverlapSize), 0, groundMask);
                break;
            case HitboxSide.down:
                groundCols = PhysicsToric.OverlapBoxAll(new Vector2(transform.position.x, transform.position.y - (hitbox.size.y + collisionOverlapSize) * 0.5f), new Vector2(hitbox.size.x, collisionOverlapSize), 0, groundMask);
                break;
            case HitboxSide.left:
                groundCols = PhysicsToric.OverlapBoxAll(new Vector2(transform.position.x - (hitbox.size.x + collisionOverlapSize) * 0.5f, transform.position.y), new Vector2(collisionOverlapSize, hitbox.size.y), 0, groundMask);
                break;
            case HitboxSide.right:
                groundCols = PhysicsToric.OverlapBoxAll(new Vector2(transform.position.x + (hitbox.size.x + collisionOverlapSize) * 0.5f, transform.position.y), new Vector2(collisionOverlapSize, hitbox.size.y), 0, groundMask);
                break;
            default:
                groundCol = null;
                return false;
        }

        foreach (Collider2D col in groundCols)
        {
            if (col != hitbox)
            {
                groundCol = col;
                return true;
            }
        }
        groundCol = null;
        return false;
    }

    #endregion

    #region CalculateTargetPos

    private Vector2 CalculateTargetPos(HitboxSide side, Collider2D groundCol)
    {
        switch (side)
        {
            case HitboxSide.up:
                return HandleUp();
            case HitboxSide.down:
                return HandleDown();
            case HitboxSide.left:
                return HandleLeft();
            case HitboxSide.right:
                return HandleRight();
            default:
                return (Vector2)transform.position;
        }

        Vector2 HandleUp()
        {
            Vector2 offset = hitbox.size.y.Round().IsOdd() ? Vector2.zero : new Vector2(0f, -LevelMapData.currentMap.cellSize.y * 0.5f);
            if (Useful.IsOdd(hitbox.size.x.Round()))
            {
                return GetCellCenter(new Vector2(transform.position.x, transform.position.y + collisionOverlapSize)) + offset;
            }
            else
            {
                return GetCellCenter(new Vector2(transform.position.x + LevelMapData.currentMap.cellSize.x * 0.5f, transform.position.y + collisionOverlapSize)) - new Vector2(LevelMapData.currentMap.cellSize.x * 0.5f, 0f) + offset;
            }
        }

        Vector2 HandleDown()
        {
            Vector2 offset = hitbox.size.y.Round().IsOdd() ? Vector2.zero : new Vector2(0f, LevelMapData.currentMap.cellSize.y * 0.5f);
            if (Useful.IsOdd(hitbox.size.x.Round()))
            {
                return GetCellCenter(new Vector2(transform.position.x, transform.position.y - collisionOverlapSize)) + offset;
            }
            else
            {
                return GetCellCenter(new Vector2(transform.position.x + LevelMapData.currentMap.cellSize.x * 0.5f, transform.position.y - collisionOverlapSize)) - new Vector2(LevelMapData.currentMap.cellSize.x * 0.5f, 0f) + offset;
            }
        }

        Vector2 HandleRight()
        {
            return new Vector2(transform.position.x + (Mathf.Abs(transform.position.x - groundCol.ClosestPoint(transform.position).x) - hitbox.size.x), transform.position.y);
        }

        Vector2 HandleLeft()
        {
            return new Vector2(transform.position.x - (Mathf.Abs(transform.position.x - groundCol.ClosestPoint(transform.position).x) - hitbox.size.x), transform.position.y);
        }

        Vector2 GetCellCenter(in Vector2 position)
        {
            Vector2 origin = PhysicsToric.GetPointInsideBounds(position) + LevelMapData.currentMap.mapSize * 0.5f;
            Vector2 coord = new Vector2((int)(origin.x / LevelMapData.currentMap.cellSize.x), (int)(origin.y / LevelMapData.currentMap.cellSize.y));
            return LevelMapData.currentMap.cellSize * coord + LevelMapData.currentMap.cellSize * 0.5f - LevelMapData.currentMap.mapSize * 0.5f;
        }
    }


    #endregion

    #region Triggers

    private void OnActivated(GameObject player, HitboxSide hitboxSide)
    {
        lastCharActivatePlateform = player;
        lastCharIdActivate = player.GetComponent<PlayerCommon>().id;
        lastTimeActivated = Time.time;
        lastSideActiavated = hitboxSide;
        Movement movement = player.GetComponent<Movement>();
        movement.ApplyBump(returnBumpSpeedOnChar * convertHitboxSideToBumbDir[(int)hitboxSide]);
        isMoving = true;
    }

    private void OnStopOnGround()
    {
        rb.position = targetPosition;
        rb.velocity = Vector2.zero;
        targetPosition = Vector2.zero;
        isTargetingPosition = isMoving = false;
        charAlreadyCrush.Clear();
    }

    private void OnCrushChar(GameObject player, GameObject killer)
    {
        charAlreadyCrush.Add(player.GetComponent<PlayerCommon>().id);
        player.GetComponent<EventController>().OnBeenKillInstant(killer);
        killer.GetComponent<EventController>().OnKill(player);
    }

    #endregion

    #region Gizmos/OnValidate

    private void Disable()
    {
        enableBehaviour = false;
        pauseData = new PauseData(Time.time - lastTimeActivated);
        pauseWasEnableLastFrame = true;
    }

    private void Enable()
    {
        enableBehaviour = true;
    }

    private void OnDestroy()
    {
        PauseManager.instance.callBackOnPauseEnable -= Disable;
        PauseManager.instance.callBackOnPauseDisable -= Enable;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        hitbox = GetComponent<BoxCollider2D>();
        this.transform = base.transform;

        collisionOverlapSize = Mathf.Max(collisionOverlapSize, 0f);
        crushPadding = Mathf.Max(crushPadding, 1f);
        dashHitbox = new Vector2(Mathf.Max(dashHitbox.x, 0f), Mathf.Max(dashHitbox.y, 0f));
        activationCooldown = Mathf.Max(activationCooldown, 0f);
        accelerationDuration = Mathf.Max(accelerationDuration, 0f);
        maxSpeed = Mathf.Max(0f, maxSpeed);
        returnBumpSpeedOnChar = Mathf.Max(0f, returnBumpSpeedOnChar);
    }

    [SerializeField] private HitboxSide hitboxSideTEST;
    private Vector2 target;
    public bool calculateTarget = true;

    private void OnDrawGizmosSelected()
    {
        if(!drawGizmos)
            return;

        hitbox = GetComponent<BoxCollider2D>();
        this.transform = base.transform;

        if(Application.isPlaying && IsGroundCollision(hitboxSideTEST, out Collider2D c) && calculateTarget)
        {
            target = CalculateTargetPos(hitboxSideTEST, c);
        }
        Gizmos.color = Color.red;
        Circle.GizmosDraw(target, 0.3f);

        if (drawGroundDetectionHitoxGizmos)
        {
            Gizmos.color = colorGroundDetectionHitbox;
            Vector2 center = new Vector2(transform.position.x, transform.position.y + (hitbox.size.y + collisionOverlapSize) * 0.5f);
            Vector2 size = new Vector2(hitbox.size.x, collisionOverlapSize);
            Hitbox.GizmosDraw(center, size);

            center = new Vector2(transform.position.x, transform.position.y - (hitbox.size.y + collisionOverlapSize) * 0.5f);
            size = new Vector2(hitbox.size.x, collisionOverlapSize);
            Hitbox.GizmosDraw(center, size);

            center = new Vector2(transform.position.x - (hitbox.size.x + collisionOverlapSize) * 0.5f, transform.position.y);
            size = new Vector2(collisionOverlapSize, hitbox.size.y);
            Hitbox.GizmosDraw(center, size);

            center = new Vector2(transform.position.x + (hitbox.size.x + collisionOverlapSize) * 0.5f, transform.position.y);
            size = new Vector2(collisionOverlapSize, hitbox.size.y);
            Hitbox.GizmosDraw(center, size);
        }

        if (drawCrushHiboxGizmos)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Char");
            if(player != null) 
            {
                Vector2 charSize = player.GetComponent<BoxCollider2D>().size;
                Gizmos.color = colorCrushDetectionHitbox;

                Vector2 center = new Vector2(transform.position.x, transform.position.y + hitbox.size.y * 0.5f + charSize.y * crushPadding * 0.5f);
                Vector2 size = new Vector2(hitbox.size.x, charSize.y * crushPadding);
                Hitbox.GizmosDraw(center, size);

                center = new Vector2(transform.position.x, transform.position.y - hitbox.size.y * 0.5f - charSize.y * crushPadding * 0.5f);
                size = new Vector2(hitbox.size.x, charSize.y * crushPadding);
                Hitbox.GizmosDraw(center, size);

                center = new Vector2(transform.position.x - hitbox.size.x * 0.5f - charSize.x * crushPadding * 0.5f, transform.position.y);
                size = new Vector2(charSize.x * crushPadding, hitbox.size.y);
                Hitbox.GizmosDraw(center, size);

                center = new Vector2(transform.position.x + hitbox.size.x * 0.5f + charSize.x * crushPadding * 0.5f, transform.position.y);
                size = new Vector2(charSize.x * crushPadding, hitbox.size.y);
                Hitbox.GizmosDraw(center, size);
            }
            else
            {
                Debug.LogWarning("Enable a char to draw craush Hitboxes");
            }
        }

        if(drawDashHiboxGizmos)
        {
            Gizmos.color = colorDashDetectionHitbox;

            Vector2 center = new Vector2(transform.position.x, transform.position.y + dashHitbox.y * 0.25f);
            Vector2 size = new Vector2(hitbox.size.x, dashHitbox.y * 0.5f);
            Hitbox.GizmosDraw(center, size);

            center = new Vector2(transform.position.x, transform.position.y - dashHitbox.y * 0.25f);
            size = new Vector2(hitbox.size.x, dashHitbox.y * 0.5f);
            Hitbox.GizmosDraw(center, size);

            center = new Vector2(transform.position.x - dashHitbox.x * 0.25f, transform.position.y);
            size = new Vector2(dashHitbox.x * 0.5f, hitbox.size.y);
            Hitbox.GizmosDraw(center, size);

            center = new Vector2(transform.position.x + dashHitbox.x * 0.25f, transform.position.y);
            size = new Vector2(dashHitbox.x * 0.5f, hitbox.size.y);
            Hitbox.GizmosDraw(center, size);
        }
    }

#endif

    #endregion

    #region

    private struct PauseData
    {
        public float lastTimeBeginMoveDeltaTime;

        public PauseData(float lastTimeBeginMoveDeltaTime)
        {
            this.lastTimeBeginMoveDeltaTime = lastTimeBeginMoveDeltaTime;
        }
    }

    #endregion
}
