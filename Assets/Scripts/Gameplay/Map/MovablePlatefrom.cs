using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MovablePlatefrom : MonoBehaviour
{
    private enum HitboxSide
    {
        up = 0, down = 1, left = 2, right = 3, none = 4
    }

    private static Vector2Int nbCaseGrid = new Vector2Int(32, 18);
    private static Vector2 caseSize => PhysicsToric.cameraSize / nbCaseGrid;
    private static Vector2[] convertHitboxSideToDir = new Vector2[5]
    {
        new Vector2(0f, 1f),
        new Vector2(0f, -1f),
        new Vector2(-1f, 0f),
        new Vector2(1f, 0f),
        new Vector2(0f, 0f)
    };

    private BoxCollider2D hitbox;
    private new Transform transform;
    private bool isMoving, isAccelerating, isReachingTargetPosition, isShaking, oldIsShaking;
    private LayerMask charMask, groundMask;
    private float lastTimeBeginMove = -10f, lastTimeBeginShake = -10f;
    private Vector2 moveDir, reachTargetPos;
    private bool oldEnableBehaviour;
    private float accelDeltaTime;
    private Rigidbody2D rb;

    public bool enableBehaviour = true;
    [SerializeField] private Vector2Int hitboxSize = new Vector2Int(1, 1);
    [SerializeField] private float charDetectionDistance;
    [SerializeField] private float groundDetectionPadding = 0.1f;
    [SerializeField] private float accelerationDuration = 1f;
    [SerializeField, Tooltip("In %age od maxSpeed")] private AnimationCurve accelerationCurve;
    [SerializeField] private float maxSpeed;
    [SerializeField] private ShakeSetting shakeSetting;

    [SerializeField] private bool gizmosDrawGrid = true;

    private void Awake()
    {
        hitbox = GetComponent<BoxCollider2D>();
        transform = base.transform;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        PauseManager.instance.callBackOnPauseDisable += Enable;
        PauseManager.instance.callBackOnPauseEnable += Disable;
        charMask = LayerMask.GetMask("Char");
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
    }

    private void Update()
    {
        if (!enableBehaviour)
        {
            if(oldEnableBehaviour)
            {
                if (isAccelerating)
                {
                    accelDeltaTime = Time.time - lastTimeBeginMove;
                }
                oldEnableBehaviour = enableBehaviour;
            }
            if(isAccelerating)
            {
                lastTimeBeginMove = Time.time - accelDeltaTime;
            }
            return;
        }
        oldEnableBehaviour = enableBehaviour;

        Vector2 caseSize = MovablePlatefrom.caseSize;
        if(isMoving)
        {
            oldIsShaking = false;
            if (isAccelerating)
            {
                Vector2 speed = maxSpeed * accelerationCurve.Evaluate(Mathf.Clamp01((Time.time - lastTimeBeginMove) / accelerationDuration)) * moveDir;
                //transform.position += (Vector3)(speed * Time.deltaTime);
                rb.velocity = speed;
                if (Time.time - lastTimeBeginMove > accelerationDuration)
                {
                    isAccelerating = false;
                }
            }
            else
            {
                //transform.position += (Vector3)(maxSpeed * Time.deltaTime * moveDir);
                rb.velocity = maxSpeed * moveDir;
            }

            //detecting ground
            if(!isReachingTargetPosition)
            {
                if (moveDir.sqrMagnitude > 1e-6f)
                {
                    (Vector2 overlapPos, Vector2 overlapSize) = GetRecInFront(transform.position, moveDir, groundDetectionPadding);
                    Collider2D groundCol = PhysicsToric.OverlapBox(overlapPos, overlapSize, 0f, groundMask);
                    if (groundCol != null)
                    {
                        reachTargetPos = Vector2.zero;
                        if (Mathf.Abs(moveDir.x) >= Mathf.Abs(moveDir.y))
                        {
                            Vector2 tmp = GetCasePos(GetCase(new Vector2(overlapPos.x - overlapSize.x * 0.5f * moveDir.x.Sign(), overlapPos.y)));
                            reachTargetPos = new Vector2(tmp.x - (0.5f * hitboxSize.x * caseSize.x) * moveDir.x.Sign() + 0.5f * caseSize.x * moveDir.x.Sign(), overlapPos.y);
                        }
                        else
                        {
                            Vector2 tmp = GetCasePos(GetCase(new Vector2(overlapPos.x, overlapPos.y - overlapSize.y * 0.5f * moveDir.y.Sign())));
                            reachTargetPos = new Vector2(overlapPos.x, tmp.y - (0.5f * hitboxSize.y * caseSize.y) * moveDir.y.Sign() + 0.5f * caseSize.y * moveDir.y.Sign());
                        }
                        isReachingTargetPosition = true;
                    }
                }
                else
                {
                    isReachingTargetPosition = isMoving = isAccelerating = false;
                }
            }
            else
            {
                rb.velocity = maxSpeed * (reachTargetPos - (Vector2)transform.position).normalized;
                //transform.position = Vector2.MoveTowards(transform.position, reachTargetPos, maxSpeed * Time.deltaTime);
                if(reachTargetPos.SqrDistance(transform.position) <= 4f * maxSpeed * maxSpeed * Time.deltaTime * Time.deltaTime)
                {
                    isReachingTargetPosition = isMoving = isAccelerating = false;
                    transform.position = reachTargetPos;
                    rb.velocity = Vector2.zero;
                    rb.constraints = RigidbodyConstraints2D.FreezeAll;
                }
            }
        }
        else if(isShaking)
        {
            if(!oldIsShaking)
            {
                transform.DOComplete();
                transform.DOShakePosition(shakeSetting.duration, shakeSetting.strengh, shakeSetting.vibrato, shakeSetting.randomness, shakeSetting.snapping, shakeSetting.fadeOut);
                oldIsShaking = true;
            }

            if(Time.time - lastTimeBeginShake > shakeSetting.duration)
            {
                isShaking = false;
                isAccelerating = isMoving = true;
            }
        }
        else
        {
            Collider2D[] cols = PhysicsToric.OverlapBoxAll(transform.position, hitbox.size + 2f * charDetectionDistance * Vector2.one, 0f, charMask);
            foreach (Collider2D col  in cols)
            {
                if(col.CompareTag("Char"))
                {
                    GameObject player = col.GetComponent<ToricObject>().original;
                    FightController fc = player.GetComponent<FightController>();
                    if (fc.isDashing)
                    {
                        BoxCollider2D charHitbox = player.GetComponent<BoxCollider2D>();
                        HitboxSide side = GetHitboxSide((Vector2)player.transform.position + charHitbox.offset, charHitbox.size);
                        if(side != HitboxSide.none)
                        {
                            isShaking = true;
                            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                            moveDir = convertHitboxSideToDir[(int)side];
                            lastTimeBeginShake = Time.time;
                        }
                    }
                }
            }
        }
    }

    #region Grid function

    private Vector2Int GetCase(in Vector2 pos)
    {
        Vector2 caseSize = MovablePlatefrom.caseSize;
        return new Vector2Int(Useful.ClampModulo(0, nbCaseGrid.x, (int)((pos.x + 0.5f * PhysicsToric.cameraSize.x) / caseSize.x)), (int)((pos.y + 0.5f * PhysicsToric.cameraSize.y) / caseSize.y));
    }

    private Vector2 GetCasePos(in Vector2Int casePos)
    {
        Vector2 caseSize = MovablePlatefrom.caseSize;
        return PhysicsToric.GetPointInsideBounds(new Vector2((casePos.x + 0.5f) * caseSize.x - 0.5f * PhysicsToric.cameraSize.x, (casePos.y + 0.5f) * caseSize.y - 0.5f * PhysicsToric.cameraSize.y));
    }

    private (Vector2, Vector2) GetRecInFront(in Vector2 pos, in Vector2 dir, float padding)//ok
    {
        Vector2 overlapPos = Vector2.zero, overlapSize = Vector2.zero;
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
        {
            if (hitboxSize.y.IsEven())//pair
            {
                Vector2 caseUpPos = GetCasePos(GetCase(new Vector2(pos.x, pos.y + 0.5f * caseSize.y)));
                overlapPos = new Vector2(pos.x + (0.5f * (hitboxSize.x + 1) * caseSize.x) * dir.x.Sign(), caseUpPos.y - 0.5f * caseSize.y);
            }
            else
            {
                Vector2 casePos = GetCasePos(GetCase(pos));
                overlapPos = new Vector2(pos.x + (0.5f * (hitboxSize.x + 1) * caseSize.x) * dir.x.Sign(), casePos.y);
            }
            overlapSize = new Vector2(caseSize.x - 2f * padding, Mathf.Max(0f, hitboxSize.y * caseSize.y - 2f * padding));
        }
        else
        {
            if (hitboxSize.x.IsEven())//pair
            {
                Vector2 caseRightPos = GetCasePos(GetCase(new Vector2(pos.x + 0.5f * caseSize.x, pos.y)));
                overlapPos = new Vector2(Mathf.Max(0f, caseRightPos.x - 0.5f * caseSize.x), pos.y + (0.5f * (hitboxSize.y + 1) * caseSize.y) * dir.y.Sign());
            }
            else
            {
                Vector2 casePos = GetCasePos(GetCase(pos));
                overlapPos = new Vector2(casePos.x, pos.y + (0.5f * (hitboxSize.y + 1) * caseSize.y) * dir.y.Sign());
            }
            overlapSize = new Vector2(Mathf.Max(0f, hitboxSize.x * caseSize.x - 2f * padding), Mathf.Max(0f, caseSize.y - 2f * padding));
        }
        return (overlapPos, overlapSize);
    }

    #endregion

    #region GetHitboxSide

    private HitboxSide GetHitboxSide(in Vector2 pos, in Vector2 size, bool debug = false)
    {
        Vector2 hitSize = hitbox.size;

        bool upOrDown = pos.x >= transform.position.x - 0.5f * hitSize.x && pos.x <= transform.position.x + 0.5f * hitSize.x;
        bool rightOrLeft = pos.y >= transform.position.y - 0.5f * hitSize.y && pos.y <= transform.position.y + 0.5f * hitSize.y;

        if(upOrDown || rightOrLeft)
        {
            if(upOrDown && rightOrLeft)
            {
                //pos is inside the hitbox
                float distEdgeX = hitSize.x - Mathf.Abs(pos.x - transform.position.x);
                float distEdgeY = hitSize.y - Mathf.Abs(pos.y - transform.position.y);

                if(distEdgeX <= distEdgeY)
                {
                    return pos.y >= transform.position.y ? HitboxSide.right : HitboxSide.left;
                }
                return pos.x >= transform.position.x ? HitboxSide.up : HitboxSide.down;
            }

            if (upOrDown)
            {
                return pos.y >= transform.position.y ? HitboxSide.up : HitboxSide.down;
            }
            return pos.x >= transform.position.x ? HitboxSide.right : HitboxSide.left;
        }

        //!ez case : We get the corners of the hitbox, calculate the closest point width the platefrom hitbox and get the side where this points is
        Vector2[] corners = new Vector2[4]
        {
            new Vector2(pos.x - size.x * 0.5f, pos.y - size.y * 0.5f),//left, bot
            new Vector2(pos.x + size.x * 0.5f, pos.y - size.y * 0.5f),//right, bot
            new Vector2(pos.x - size.x * 0.5f, pos.y + size.y * 0.5f),//left, up
            new Vector2(pos.x + size.x * 0.5f, pos.y + size.y * 0.5f)//right, up
        };

        Vector2 closestPoint = hitbox.ClosestPoint(corners[0]);
        float closestPointSqrDist = corners[0].SqrDistance(closestPoint);
        int closestPointIndex = 0;

        for (int i = 1; i < 4; i++)
        {
            Vector2 tmp = hitbox.ClosestPoint(corners[i]);
            float sqrDist = corners[i].SqrDistance(tmp);
            if(sqrDist < closestPointSqrDist)
            {
                closestPointSqrDist = sqrDist;
                closestPoint = tmp;
                closestPointIndex = i;
            }
        }

        if(debug)
        {
            Vector2[] vertices = new Vector2[4]
            {
                new Vector2(hitbox.transform.position.x - hitbox.size.x * 0.5f, hitbox.transform.position.y - hitbox.size.y * 0.5f),//left, bot
                new Vector2(hitbox.transform.position.x + hitbox.size.x * 0.5f, hitbox.transform.position.y - hitbox.size.y * 0.5f),//right, bot
                new Vector2(hitbox.transform.position.x - hitbox.size.x * 0.5f, hitbox.transform.position.y + hitbox.size.y * 0.5f),//left, up
                new Vector2(hitbox.transform.position.x + hitbox.size.x * 0.5f, hitbox.transform.position.y + hitbox.size.y * 0.5f)//right, up
            };

            Gizmos.color = Color.blue;
            foreach(Vector2 v in vertices)
            {
                Circle.GizmosDraw(v, 0.2f);
            }

            Gizmos.color = Color.black;
            foreach (Vector2 v in corners)
            {
                Circle.GizmosDraw(v, 0.2f);
            }

            Gizmos.color = Color.red;
            Circle.GizmosDraw(closestPoint, 0.3f);
        }

        foreach (Vector2 corner in corners)
        {
            if(corner.SqrDistance(closestPoint) < 1e-6f)
            {
                print("corner bug");
            }
        }

        if (closestPoint.x >= transform.position.x - 0.5f * hitSize.x && closestPoint.x <= transform.position.x + 0.5f * hitSize.x)
        {
            return closestPoint.y >= transform.position.y ? HitboxSide.up : HitboxSide.down;
        }
        if (closestPoint.y >= transform.position.y - 0.5f * hitSize.y && closestPoint.y <= transform.position.y + 0.5f * hitSize.y)
        {
            return closestPoint.x >= transform.position.x ? HitboxSide.right : HitboxSide.left;
        }

        Debug.LogWarning("Debug pls");
        return HitboxSide.none;
    }

    #endregion

    #region Gizmos/OnValidate

    private void Disable()
    {
        enableBehaviour = false;
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

    private void OnValidate()
    {
        if (hitbox == null)
            hitbox = GetComponent<BoxCollider2D>();
        transform = base.transform;

        Vector2 caseSize = MovablePlatefrom.caseSize;
        hitboxSize = new Vector2Int(Useful.Max(0, hitboxSize.x), Useful.Max(0, hitboxSize.y));
        hitbox.size = caseSize * hitboxSize;
        accelerationDuration = Mathf.Max(0f, accelerationDuration);
        maxSpeed = Mathf.Max(0f, maxSpeed);
        groundDetectionPadding = Mathf.Max(0f, groundDetectionPadding);
        shakeSetting.ClampValue();
    }

    private void OnDrawGizmosSelected()
    {
        if(gizmosDrawGrid)
        {
            Vector2 caseSize = MovablePlatefrom.caseSize;
            Vector2 offset = -0.5f * PhysicsToric.cameraSize + 0.5f * caseSize;
            Gizmos.color = Color.red;
            for (int y = 0; y < nbCaseGrid.y; y++)
            {
                for (int x = 0; x < nbCaseGrid.x; x++)
                {
                    Hitbox.GizmosDraw(offset + new Vector2(caseSize.x * x, caseSize.y * y), caseSize);
                }
            }
        }

        //hitbox
        Gizmos.color = Color.green;
        Hitbox.GizmosDraw(transform.position, hitbox.size * caseSize);

        //chardetection
        Hitbox.GizmosDraw(transform.position, hitbox.size * caseSize + 2f * charDetectionDistance * Vector2.one);

        //test GetRecInFront
        (Vector2 pos, Vector2 size) = GetRecInFront(transform.position, Vector2.right, groundDetectionPadding);
        Gizmos.color = Color.blue;
        Hitbox.GizmosDraw(pos, size);
        (pos, size) = GetRecInFront(transform.position, Vector2.left, groundDetectionPadding);
        Hitbox.GizmosDraw(pos, size);
        (pos, size) = GetRecInFront(transform.position, Vector2.up, groundDetectionPadding);
        Hitbox.GizmosDraw(pos, size);
        (pos, size) = GetRecInFront(transform.position, Vector2.down, groundDetectionPadding);
        Hitbox.GizmosDraw(pos, size);
    }

    /*
    private void OnDrawGizmos()
    {
        //test GetHitboxSide
        GameObject char1 = GameObject.FindGameObjectsWithTag("Char").Where((GameObject go) => go.GetComponent<PlayerCommon>().id == 0).First();
        HitboxSide hitboxSide = GetHitboxSide((Vector2)char1.transform.position + char1.GetComponent<BoxCollider2D>().offset, char1.GetComponent<BoxCollider2D>().size, true);
        print(hitboxSide);
    }
    */
    #endregion

}
