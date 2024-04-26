using UnityEngine;
using System.Collections.Generic;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;
using PathFinding;

[RequireComponent(typeof(BoxCollider2D))]
public class MovablePlateform : PathFindingBlocker
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
    private GameObject lastCharActivatePlateform;
    private uint lastCharIdActivate;
    private Vector2[] convertHitboxSideToBumbDir;
    private float lastTimeActivated = -10f;
    private HitboxSide lastSideActiavated = HitboxSide.none;
    private Vector2 targetPosition;
    private List<uint> charAlreadyCrush;
    private PauseData pauseData;
    private bool pauseWasEnableLastFrame;
    private ToricObject toricObject;
    private MapColliderData mapColliderData;
    private Vector2 velocity;

    public bool enableBehaviour = true;
    [SerializeField] private bool enableLeftAndRightDash, enableUpAndDownDash;
    [SerializeField, Range(0f, 1f)] private float groundCollisionHitboxSafeZone = 1f;
    [SerializeField, Tooltip("La marge d'erreur de détection de crush (%age total)")] private float crushPadding = 1.05f;
    [SerializeField, Range(0f, 1f), Tooltip("La marge d'erreur de détection de crush (%age total)")] private float crushZone = 0.95f;
    [SerializeField, Tooltip("The dash detection hitbox size")] private float dashHitboxSize = 1f;
    [SerializeField, Tooltip("The dash detection hitbox size"), Range(0f, 1f)] private float dashHitboxSafeZone = 0.9f;
    [SerializeField] private bool enableActivationWhenMoving;
    [SerializeField] private float activationCooldown = 0.3f;
    [SerializeField] private float accelerationDuration = 1f;
    [SerializeField, Tooltip("In %age of maxSpeed")] private AnimationCurve accelerationCurve;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float sideReturnBumpSpeedOnChar = 5f;
    [SerializeField] private float returnBumpSpeedOnChar = 5f;
    [SerializeField, Range(0f, 90f)] private float returnBumpSpeedAngleOnSide = 45f;

#if UNITY_EDITOR

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private bool drawGroundDetectionHitoxGizmos = true, drawCrushHiboxSimpleGizmos = true, drawCrushHitboxAccurate = true, drawDashHiboxGizmos = true;
    [SerializeField] private Color colorGroundDetectionHitbox, colorCrushDetectionHitbox, colorDashDetectionHitbox;

#endif

    #region PathFinding

    public override List<MapPoint> GetBlockedCells(Map map)
    {
        return GetBlockedCellsInRectangle(map, transform.position, hitbox.size - LevelMapData.currentMap.cellSize * 0.1f);
    }

    #endregion

    #region Awake and Start

    protected override void Awake()
    {
        base.Awake();
        transform = base.transform;
        hitbox = GetComponent<BoxCollider2D>();
        convertHitboxSideToBumbDir = new Vector2[5]
        {
            Vector2.up,
            Vector2.down,
            Useful.Vector2FromAngle((180f - returnBumpSpeedAngleOnSide) * Mathf.Deg2Rad),
            Useful.Vector2FromAngle(returnBumpSpeedAngleOnSide * Mathf.Deg2Rad),
            Vector2.zero
        };

        charAlreadyCrush = new List<uint>();
        toricObject = GetComponent<ToricObject>();
        mapColliderData = GetComponent<MapColliderData>();
    }

    private void Start()
    {
        PhysicsToric.AddPriorityCollider(hitbox);
        PauseManager.instance.callBackOnPauseDisable += Enable;
        PauseManager.instance.callBackOnPauseEnable += Disable;
        charMask = LayerMask.GetMask("Char");
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
    }

    #endregion

    #region Update

    private void Update()
    {
        if(!enableBehaviour || toricObject.isAClone)
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

            bool IsGroundCollision(HitboxSide hitboxSide, out Collider2D groundCol)
            {
                Collider2D[] groundCols;
                switch (hitboxSide)
                {
                    case HitboxSide.up:
                        groundCols = PhysicsToric.OverlapBoxAll(new Vector2(transform.position.x, transform.position.y + (hitbox.size.y + LevelMapData.currentMap.cellSize.y) * 0.5f), new Vector2(hitbox.size.x * groundCollisionHitboxSafeZone, LevelMapData.currentMap.cellSize.y), 0, groundMask);
                        break;
                    case HitboxSide.down:
                        groundCols = PhysicsToric.OverlapBoxAll(new Vector2(transform.position.x, transform.position.y - (hitbox.size.y + LevelMapData.currentMap.cellSize.y) * 0.5f), new Vector2(hitbox.size.x * groundCollisionHitboxSafeZone, LevelMapData.currentMap.cellSize.y), 0, groundMask);
                        break;
                    case HitboxSide.left:
                        groundCols = PhysicsToric.OverlapBoxAll(new Vector2(transform.position.x - (hitbox.size.x + LevelMapData.currentMap.cellSize.x) * 0.5f, transform.position.y), new Vector2(LevelMapData.currentMap.cellSize.x, hitbox.size.y * groundCollisionHitboxSafeZone), 0, groundMask);
                        break;
                    case HitboxSide.right:
                        groundCols = PhysicsToric.OverlapBoxAll(new Vector2(transform.position.x + (hitbox.size.x + LevelMapData.currentMap.cellSize.x) * 0.5f, transform.position.y), new Vector2(LevelMapData.currentMap.cellSize.x, hitbox.size.y * groundCollisionHitboxSafeZone), 0, groundMask);
                        break;
                    default:
                        groundCol = null;
                        return false;
                }

                foreach (Collider2D col in groundCols)
                {
                    if (col != hitbox)
                    {
                        ToricObject tc = col.GetComponent<ToricObject>();
                        if (tc == null || tc.original != gameObject)
                        {
                            groundCol = col;
                            return true;
                        }
                    }
                }
                groundCol = null;
                return false;
            }

            //speed
            if (isTargetingPosition)
            {
                velocity = Vector2.zero;
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, maxSpeed * Time.fixedDeltaTime);

                if(((Vector2)transform.position).SqrDistance(targetPosition) < 4f * Time.fixedDeltaTime * Time.fixedDeltaTime * maxSpeed * maxSpeed)
                {
                    OnStopOnGround();
                }
            }
            else
            {
                float speed = Time.time - lastTimeActivated > accelerationDuration ? maxSpeed : maxSpeed * accelerationCurve.Evaluate((Time.time - lastTimeActivated) / accelerationDuration);
                velocity = speed * convertHitboxSideToDir[(int)lastSideActiavated];
            }

            //char dash detecttion
            if(enableActivationWhenMoving && !isTargetingPosition)
            {
                Collider2D[] cols;
                bool dashAlreadyCollide = false;
                if (enableUpAndDownDash)
                {
                    //up
                    cols = GetCharColliders(HitboxSide.up);
                    dashAlreadyCollide = HandleDashCollision(cols, HitboxSide.up);
                    //down
                    if(!dashAlreadyCollide)
                    {
                        cols = GetCharColliders(HitboxSide.down);
                        dashAlreadyCollide = HandleDashCollision(cols, HitboxSide.down);
                    }
                }

                if (enableLeftAndRightDash)
                {
                    if (!dashAlreadyCollide)
                    {
                        //right
                        cols = GetCharColliders(HitboxSide.right);
                        dashAlreadyCollide = HandleDashCollision(cols, HitboxSide.right);
                    }
                    if (!dashAlreadyCollide)
                    {
                        //left
                        cols = GetCharColliders(HitboxSide.left);
                        dashAlreadyCollide = HandleDashCollision(cols, HitboxSide.left);
                    }
                }

                bool HandleDashCollision(Collider2D[] cols, HitboxSide hitboxSide)
                {
                    if (lastSideActiavated == hitboxSide)
                        return false;

                    foreach (Collider2D col in cols)
                    {
                        if (col.CompareTag("Char"))
                        {
                            GameObject player = col.GetComponent<ToricObject>().original;
                            if (IsPlayerDash(player))
                            {
                                if (Time.time - lastTimeActivated > activationCooldown)
                                {
                                    velocity = Vector2.zero;
                                    OnActivated(player, hitboxSide);
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }
            }

            //Crushing Char
            if(CrushDetectionSimple(lastSideActiavated))
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
                        size = new Vector2(hitbox.size.x * crushZone, PlayerCommon.charSize.y * crushPadding);
                        break;
                    case HitboxSide.down:
                        center = new Vector2(transform.position.x, transform.position.y - hitbox.size.y * 0.5f - PlayerCommon.charSize.y * crushPadding * 0.5f);
                        size = new Vector2(hitbox.size.x * crushZone, PlayerCommon.charSize.y * crushPadding);
                        break;
                    case HitboxSide.left:
                        center = new Vector2(transform.position.x - hitbox.size.x * 0.5f - PlayerCommon.charSize.x * crushPadding * 0.5f, transform.position.y);
                        size = new Vector2(PlayerCommon.charSize.x * crushPadding, hitbox.size.y * crushZone);
                        break;
                    case HitboxSide.right:
                        center = new Vector2(transform.position.x + hitbox.size.x * 0.5f + PlayerCommon.charSize.x * crushPadding * 0.5f, transform.position.y);
                        size = new Vector2(PlayerCommon.charSize.x * crushPadding, hitbox.size.y * crushZone);
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
                        nbStep = (hitbox.size.x * crushZone / LevelMapData.currentMap.cellSize.x).Ceil();
                        size = new Vector2(hitbox.size.x * crushZone / nbStep, PlayerCommon.charSize.y * crushPadding);
                        begPoint = new Vector2(transform.position.x - (nbStep * 0.5f - 0.5f) * size.x, transform.position.y + hitbox.size.y * 0.5f + size.y * 0.5f);
                        step = new Vector2(size.x, 0f);
                        break;
                    case HitboxSide.down:
                        nbStep = (hitbox.size.x * crushZone / LevelMapData.currentMap.cellSize.x).Ceil();
                        size = new Vector2(hitbox.size.x * crushZone / nbStep, PlayerCommon.charSize.y * crushPadding);
                        begPoint = new Vector2(transform.position.x - (nbStep * 0.5f - 0.5f) * size.x, transform.position.y - hitbox.size.y * 0.5f - size.y * 0.5f);
                        step = new Vector2(size.x, 0f);
                        break;
                    case HitboxSide.left:
                        nbStep = (hitbox.size.y * crushZone / LevelMapData.currentMap.cellSize.y).Ceil();
                        size = new Vector2(PlayerCommon.charSize.x * crushPadding, hitbox.size.y * crushZone / nbStep);
                        begPoint = new Vector2(transform.position.x - hitbox.size.x * 0.5f - size.x * 0.5f, transform.position.y + (nbStep * 0.5f - 0.5f) * size.y);
                        step = new Vector2(0f, -size.y);
                        break;
                    case HitboxSide.right:
                        nbStep = (hitbox.size.y * crushZone / LevelMapData.currentMap.cellSize.y).Ceil();
                        size = new Vector2(PlayerCommon.charSize.x * crushPadding, hitbox.size.y * crushZone / nbStep);
                        begPoint = new Vector2(transform.position.x + hitbox.size.x * 0.5f + size.x * 0.5f, transform.position.y + (nbStep * 0.5f - 0.5f) * size.y);
                        step = new Vector2(0f, -size.y);
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
                        if(col == hitbox)
                        {
                            continue;
                        }

                        ToricObject colTO = col.GetComponent<ToricObject>();
                        if(colTO == null || colTO.original != toricObject.original)
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
                    begPoint += step;
                }
            }
        }

        void HandleStay()
        {
            //detect char dash
            Collider2D[] cols;
            if (enableUpAndDownDash)
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
                                break;
                            }
                        }
                    }
                }
            }
        }

        bool IsPlayerDash(GameObject player)
        {
            return player.GetComponent<CharacterController>().isDashing;
        }

        Collider2D[] GetCharColliders(HitboxSide hitboxSide)
        {
            Vector2 center, size;
            switch (hitboxSide)
            {
                case HitboxSide.up:
                    center = new Vector2(transform.position.x, transform.position.y + (hitbox.size.y + dashHitboxSize) * 0.5f);
                    size = new Vector2(hitbox.size.x * dashHitboxSafeZone, dashHitboxSize);
                    break;
                case HitboxSide.down:
                    center = new Vector2(transform.position.x, transform.position.y - (hitbox.size.y + dashHitboxSize) * 0.5f);
                    size = new Vector2(hitbox.size.x * dashHitboxSafeZone, dashHitboxSize);
                    break;
                case HitboxSide.left:
                    center = new Vector2(transform.position.x - (hitbox.size.x + dashHitboxSize) * 0.5f, transform.position.y);
                    size = new Vector2(dashHitboxSize, hitbox.size.y * dashHitboxSafeZone);
                    break;
                case HitboxSide.right:
                    center = new Vector2(transform.position.x + (hitbox.size.x + dashHitboxSize) * 0.5f, transform.position.y);
                    size = new Vector2(dashHitboxSize, hitbox.size.y * dashHitboxSafeZone);
                    break;
                default:
                    return System.Array.Empty<Collider2D>();
            }
            return PhysicsToric.OverlapBoxAll(center, size, 0, charMask);
        }

        transform.position += (Vector3)(velocity * Time.deltaTime);
    }

    #endregion

    #region CalculateTargetPos

    private Vector2 CalculateTargetPos(HitboxSide side, Collider2D groundCol)
    {
        switch (side)
        {
            case HitboxSide.up:
                return HandleUp(groundCol);
            case HitboxSide.down:
                return HandleDown(groundCol);
            case HitboxSide.left:
                return HandleLeft(groundCol);
            case HitboxSide.right:
                return HandleRight(groundCol);
            default:
                return (Vector2)transform.position;
        }

        Vector2 HandleUp(Collider2D groundCol)
        {
            return new Vector2(transform.position.x, transform.position.y + (Mathf.Abs(transform.position.y - groundCol.ClosestPoint(transform.position).y)) - hitbox.size.y * 0.5f);
        }

        Vector2 HandleDown(Collider2D groundCol)
        {
            return new Vector2(transform.position.x, transform.position.y - (Mathf.Abs(transform.position.y - groundCol.ClosestPoint(transform.position).y)) + hitbox.size.y * 0.5f);
        }

        Vector2 HandleRight(Collider2D groundCol)
        {
            return new Vector2(transform.position.x + (Mathf.Abs(transform.position.x - groundCol.ClosestPoint(transform.position).x)) - hitbox.size.x * 0.5f, transform.position.y);
        }

        Vector2 HandleLeft(Collider2D groundCol)
        {
            return new Vector2(transform.position.x - (Mathf.Abs(transform.position.x - groundCol.ClosestPoint(transform.position).x)) + hitbox.size.x * 0.5f, transform.position.y);
        }
    }

    private Vector2 GetCellCenter(in Vector2 position)
    {
        Vector2 origin = PhysicsToric.GetPointInsideBounds(position) + LevelMapData.currentMap.mapSize * 0.5f;
        Vector2 coord = new Vector2((int)(origin.x / LevelMapData.currentMap.cellSize.x), (int)(origin.y / LevelMapData.currentMap.cellSize.y));
        return LevelMapData.currentMap.cellSize * coord + LevelMapData.currentMap.cellSize * 0.5f - LevelMapData.currentMap.mapSize * 0.5f;
    }

    #endregion

    #region Triggers

    private void OnActivated(GameObject player, HitboxSide hitboxSide)
    {
        lastCharActivatePlateform = player;
        lastCharIdActivate = player.GetComponent<PlayerCommon>().id;
        lastTimeActivated = Time.time;
        lastSideActiavated = hitboxSide;
        CharacterController movement = player.GetComponent<CharacterController>();
        float bumpSpeed = hitboxSide == HitboxSide.right || hitboxSide == HitboxSide.left ? sideReturnBumpSpeedOnChar : returnBumpSpeedOnChar;
        movement.ApplyBump(bumpSpeed * convertHitboxSideToBumbDir[(int)hitboxSide]);
        isMoving = true;
    }

    private void OnStopOnGround()
    {
        transform.position = targetPosition;
        velocity = Vector2.zero;
        targetPosition = Vector2.zero;
        isTargetingPosition = isMoving = false;
        charAlreadyCrush.Clear();
    }

    private void OnCrushChar(GameObject player, GameObject killer)
    {
        charAlreadyCrush.Add(player.GetComponent<PlayerCommon>().id);
        player.GetComponent<EventController>().OnBeenKillInstant(killer);
        killer.GetComponent<EventController>().OnKill(player);
        EventManager.instance.OnTriggerPlayerDeath(player, killer);
    }

    #endregion

    #region Gizmos/OnValidate/Destroy

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

    protected override void OnDestroy()
    {
        base.OnDestroy();
        PauseManager.instance.callBackOnPauseEnable -= Disable;
        PauseManager.instance.callBackOnPauseDisable -= Enable;
        PhysicsToric.RemovePriorityCollider(hitbox);
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        hitbox = GetComponent<BoxCollider2D>();
        this.transform = base.transform;
        crushPadding = Mathf.Max(crushPadding, 1f);
        dashHitboxSize = Mathf.Max(dashHitboxSize, 0f);
        activationCooldown = Mathf.Max(activationCooldown, 0f);
        accelerationDuration = Mathf.Max(accelerationDuration, 0f);
        maxSpeed = Mathf.Max(0f, maxSpeed);
        returnBumpSpeedOnChar = Mathf.Max(0f, returnBumpSpeedOnChar);
    }

    private void OnDrawGizmosSelected()
    {
        if(!drawGizmos)
            return;

        hitbox = GetComponent<BoxCollider2D>();
        this.transform = base.transform;

        if (drawGroundDetectionHitoxGizmos)
        {
            Gizmos.color = colorGroundDetectionHitbox;
            Vector2 center = new Vector2(transform.position.x, transform.position.y + (hitbox.size.y + LevelMapData.currentMap.cellSize.y) * 0.5f);
            Vector2 size = new Vector2(hitbox.size.x * groundCollisionHitboxSafeZone, LevelMapData.currentMap.cellSize.y);
            Hitbox.GizmosDraw(center, size, colorGroundDetectionHitbox);

            center = new Vector2(transform.position.x, transform.position.y - (hitbox.size.y + LevelMapData.currentMap.cellSize.y) * 0.5f);
            size = new Vector2(hitbox.size.x * groundCollisionHitboxSafeZone, LevelMapData.currentMap.cellSize.y);
            Hitbox.GizmosDraw(center, size, colorGroundDetectionHitbox);

            center = new Vector2(transform.position.x - (hitbox.size.x + LevelMapData.currentMap.cellSize.x) * 0.5f, transform.position.y);
            size = new Vector2(LevelMapData.currentMap.cellSize.x, hitbox.size.y * groundCollisionHitboxSafeZone);
            Hitbox.GizmosDraw(center, size, colorGroundDetectionHitbox);

            center = new Vector2(transform.position.x + (hitbox.size.x + LevelMapData.currentMap.cellSize.x) * 0.5f, transform.position.y);
            size = new Vector2(LevelMapData.currentMap.cellSize.x, hitbox.size.y * groundCollisionHitboxSafeZone);
            Hitbox.GizmosDraw(center, size, colorGroundDetectionHitbox);
        }

        if (drawCrushHiboxSimpleGizmos)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Char");
            if(player != null) 
            {
                Vector2 charSize = player.GetComponent<BoxCollider2D>().size;
                Gizmos.color = colorCrushDetectionHitbox;

                Vector2 center = new Vector2(transform.position.x, transform.position.y + hitbox.size.y * 0.5f + charSize.y * crushPadding * 0.5f);
                Vector2 size = new Vector2(hitbox.size.x * crushZone, charSize.y * crushPadding);
                Hitbox.GizmosDraw(center, size, colorCrushDetectionHitbox);

                center = new Vector2(transform.position.x, transform.position.y - hitbox.size.y * 0.5f - charSize.y * crushPadding * 0.5f);
                size = new Vector2(hitbox.size.x * crushZone, charSize.y * crushPadding);
                Hitbox.GizmosDraw(center, size, colorCrushDetectionHitbox);

                center = new Vector2(transform.position.x - hitbox.size.x * 0.5f - charSize.x * crushPadding * 0.5f, transform.position.y);
                size = new Vector2(charSize.x * crushPadding, hitbox.size.y * crushZone);
                Hitbox.GizmosDraw(center, size, colorCrushDetectionHitbox);

                center = new Vector2(transform.position.x + hitbox.size.x * 0.5f + charSize.x * crushPadding * 0.5f, transform.position.y);
                size = new Vector2(charSize.x * crushPadding, hitbox.size.y * crushZone);
                Hitbox.GizmosDraw(center, size, colorCrushDetectionHitbox);
            }
            else
            {
                Debug.LogWarning("Enable a char to draw craush Hitboxes");
            }
        }

        if (drawCrushHitboxAccurate)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Char");
            if (player != null)
            {
                Vector2 charSize = player.GetComponent<BoxCollider2D>().size;
                Gizmos.color = colorCrushDetectionHitbox;

                int nbStep = (hitbox.size.x * crushZone / LevelMapData.currentMap.cellSize.x).Ceil();
                Vector2 size = new Vector2(hitbox.size.x * crushZone / nbStep, charSize.y * crushPadding);
                Vector2 begPoint = new Vector2(transform.position.x - (nbStep * 0.5f - 0.5f) * size.x, transform.position.y + hitbox.size.y * 0.5f + size.y * 0.5f);
                Vector2 step = new Vector2(size.x, 0f);
                DrawGizmos(begPoint, size, step, nbStep, colorCrushDetectionHitbox);

                nbStep = (hitbox.size.x * crushZone / LevelMapData.currentMap.cellSize.x).Ceil();
                size = new Vector2(hitbox.size.x * crushZone / nbStep, charSize.y * crushPadding);
                begPoint = new Vector2(transform.position.x - (nbStep * 0.5f - 0.5f) * size.x, transform.position.y - hitbox.size.y * 0.5f - size.y * 0.5f);
                step = new Vector2(size.x, 0f);
                DrawGizmos(begPoint, size, step, nbStep, colorCrushDetectionHitbox);


                nbStep = (hitbox.size.y * crushZone / LevelMapData.currentMap.cellSize.y).Ceil();
                size = new Vector2(charSize.x * crushPadding, hitbox.size.y * crushZone / nbStep);
                begPoint = new Vector2(transform.position.x - hitbox.size.x * 0.5f - size.x * 0.5f, transform.position.y + (nbStep * 0.5f - 0.5f) * size.y);
                step = new Vector2(0f, -size.y);
                DrawGizmos(begPoint, size, step, nbStep, colorCrushDetectionHitbox);


                nbStep = (hitbox.size.y * crushZone / LevelMapData.currentMap.cellSize.y).Ceil();
                size = new Vector2(charSize.x * crushPadding, hitbox.size.y * crushZone / nbStep);
                begPoint = new Vector2(transform.position.x + hitbox.size.x * 0.5f + size.x * 0.5f, transform.position.y + (nbStep * 0.5f - 0.5f) * size.y);
                step = new Vector2(0f, -size.y);
                DrawGizmos(begPoint, size, step, nbStep, colorCrushDetectionHitbox);

                void DrawGizmos(Vector2 begPoint, Vector2 size, Vector2 step, int nbStep, Color color)
                {
                    for (int i = 0; i < nbStep; i++)
                    {
                        Hitbox.GizmosDraw(begPoint, size, color);
                        begPoint += step;
                    }
                }
            }
            else
            {
                Debug.LogWarning("Enable a char to draw craush Hitboxes accurate.");
            }
        }

        if (drawDashHiboxGizmos)
        {
            Gizmos.color = colorDashDetectionHitbox;

            Vector2 center = new Vector2(transform.position.x , transform.position.y + (hitbox.size.y + dashHitboxSize) * 0.5f);
            Vector2 size = new Vector2(hitbox.size.x * dashHitboxSafeZone, dashHitboxSize);
            Hitbox.GizmosDraw(center, size, colorDashDetectionHitbox);

            center = new Vector2(transform.position.x, transform.position.y - (hitbox.size.y + dashHitboxSize) * 0.5f);
            size = new Vector2(hitbox.size.x * dashHitboxSafeZone, dashHitboxSize);
            Hitbox.GizmosDraw(center, size, colorDashDetectionHitbox);

            center = new Vector2(transform.position.x - (hitbox.size.x + dashHitboxSize) * 0.5f, transform.position.y);
            size = new Vector2(dashHitboxSize, hitbox.size.y * dashHitboxSafeZone);
            Hitbox.GizmosDraw(center, size, colorDashDetectionHitbox);

            center = new Vector2(transform.position.x + (hitbox.size.x + dashHitboxSize) * 0.5f, transform.position.y);
            size = new Vector2(dashHitboxSize, hitbox.size.y * dashHitboxSafeZone);
            Hitbox.GizmosDraw(center, size, colorDashDetectionHitbox);
        }
    }

#endif

    #endregion

    #region Custom struct

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
