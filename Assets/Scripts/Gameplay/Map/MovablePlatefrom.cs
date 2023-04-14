using System.Collections.Generic;
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
    private bool isMoving, isAccelerating;
    private LayerMask charMask;
    private float lastTimeBeginMove = -10f;
    private Vector2 moveDir;

    public bool enableBehaviour = true;
    [SerializeField] private Vector2Int hitboxSize = new Vector2Int(1, 1);
    [SerializeField] private float dashCharDetectionDistance;
    [SerializeField] private float accelerationDuration = 1f;
    [SerializeField, Tooltip("In %age od maxSpeed")] private AnimationCurve accelerationCurve;
    [SerializeField] private float maxSpeed;

    [SerializeField] private bool gizmosDrawGrid = true;

    private void Awake()
    {
        hitbox = GetComponent<BoxCollider2D>();
        transform = base.transform;
    }

    private void Start()
    {
        PauseManager.instance.callBackOnPauseDisable += Enable;
        PauseManager.instance.callBackOnPauseEnable += Disable;
        charMask = LayerMask.GetMask("Char");
    }

    private void Update()
    {
        if (!enableBehaviour)
            return;

        if(isMoving)
        {
            if(isAccelerating)
            {
                Vector2 speed = moveDir * accelerationCurve.Evaluate(Mathf.Clamp01(Time.time - lastTimeBeginMove / accelerationDuration));
                transform.position += (Vector3)(speed * Time.deltaTime);
                if (Time.time - lastTimeBeginMove > accelerationDuration)
                {
                    isAccelerating = false;
                }
            }
            else
            {
                //todo : 
            }
        }
        else
        {
            Collider2D[] cols = PhysicsToric.OverlapBoxAll(transform.position, hitbox.size + 2f * dashCharDetectionDistance * Vector2.one, 0f, charMask);
            foreach (Collider2D col  in cols)
            {
                if(col.CompareTag("Char"))
                {
                    GameObject player = col.GetComponent<ToricObject>().original;
                    FightController fc = player.GetComponent<FightController>();
                    if (fc.isDashing)
                    {
                        HitboxSide side = GetHitboxSide(player.transform.position, player.GetComponent<BoxCollider2D>().size);
                        if(side != HitboxSide.none)
                        {
                            isMoving = isAccelerating = true;
                            moveDir = convertHitboxSideToDir[(int)side];
                            lastTimeBeginMove = Time.time;
                        }
                    }
                }
            }
        }
    }

    #region GetHitboxSide

    private HitboxSide GetHitboxSide(in Vector2 pos, in Vector2 size)
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
                return pos.x >= transform.position.x ? HitboxSide.up : HitboxSide.down;
            }
            return pos.y >= transform.position.y ? HitboxSide.right : HitboxSide.left;
        }

        //!ez case : We get the corners of the hitbox, calculate the closest point width the platefrom hitbox and get the side where this points is
        Vector2[] corners = new Vector2[4]
        {
            new Vector2(pos.x - size.x * 0.5f, pos.y - size.y * 0.5f),
            new Vector2(pos.x + size.x * 0.5f, pos.y - size.y * 0.5f),
            new Vector2(pos.x - size.x * 0.5f, pos.y + size.y * 0.5f),
            new Vector2(pos.x + size.x * 0.5f, pos.y + size.y * 0.5f)
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

        if (closestPoint.x >= transform.position.x - 0.5f * hitSize.x && closestPoint.x <= transform.position.x + 0.5f * hitSize.x)
        {
            return closestPoint.x >= transform.position.x ? HitboxSide.up : HitboxSide.down;
        }
        if (closestPoint.y >= transform.position.y - 0.5f * hitSize.y && closestPoint.y <= transform.position.y + 0.5f * hitSize.y)
        {
            return closestPoint.y >= transform.position.y ? HitboxSide.right : HitboxSide.left;
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
        hitbox.size = caseSize * hitboxSize;
        accelerationDuration = Mathf.Max(0f, accelerationDuration);
        maxSpeed = Mathf.Max(0f, maxSpeed);
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

        //chardetection
        Gizmos.color = Color.green;
        Hitbox.GizmosDraw(transform.position, hitbox.size + 2f * dashCharDetectionDistance * Vector2.one);
    }

    #endregion
}
