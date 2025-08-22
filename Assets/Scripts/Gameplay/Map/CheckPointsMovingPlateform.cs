using System.Collections.Generic;
using UnityEngine;
using Collision2D;

[RequireComponent(typeof(BoxCollider2D))]
public class CheckPointsMovingPlateform : MonoBehaviour
{
    private bool isWaiting;
    private int targetIndex;
    private float lastTimeBeginWait;
    private Vector2 dir;
    private BoxCollider2D mainHitbox;
    private Vector2 offsetHitboxUp, sizeHitboxUp;
    private Vector2 offsetHitboxDown, sizeHitboxDown;
    private Vector2 offsetHitboxRight, sizeHitboxRight;
    private Vector2 offsetHitboxLeft, sizeHitboxLeft;
    private List<uint> charAlreadyTouch;
    private LayerMask charMask, groundMask;

    public bool enableBehaviour = true;
    [SerializeField] private Vector2[] checkPoints;
    [SerializeField] private float[] waitingTimeCheckPoints;
    [SerializeField] private float[] speeds;
    [SerializeField] private float detectionHitboxSize;

    private void Awake()
    {
        mainHitbox = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        if(checkPoints.Length != waitingTimeCheckPoints.Length || checkPoints.Length != speeds.Length)
        {
            Debug.LogWarning("checkPoints, waitingTimeCheckPoints and speeds must have the samme length");
        }

        transform.position = checkPoints[0];
        lastTimeBeginWait = Time.time;
        isWaiting = true;
        targetIndex = 0;
        charAlreadyTouch = new List<uint>();
        charMask = LayerMask.GetMask("Char");
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");

        CalculateDetectionHitbox();

        PauseManager.instance.callBackOnPauseDisable += Enable;
        PauseManager.instance.callBackOnPauseEnable += Disable;
    }

    private void CalculateDetectionHitbox()
    {
        offsetHitboxUp = new Vector2(0f, (mainHitbox.size.y + detectionHitboxSize) * 0.5f);
        offsetHitboxDown = new Vector2(0f, -(mainHitbox.size.y + detectionHitboxSize) * 0.5f);
        offsetHitboxRight = new Vector2((mainHitbox.size.x + detectionHitboxSize) * 0.5f, 0f);
        offsetHitboxLeft = new Vector2(-(mainHitbox.size.x + detectionHitboxSize) * 0.5f, 0f);
        sizeHitboxUp = new Vector2(mainHitbox.size.x, detectionHitboxSize);
        sizeHitboxDown = new Vector2(mainHitbox.size.x, detectionHitboxSize);
        sizeHitboxRight = new Vector2(detectionHitboxSize, mainHitbox.size.y);
        sizeHitboxLeft = new Vector2(detectionHitboxSize, mainHitbox.size.y);
    }

    private void Update()
    {
        if (!enableBehaviour)
            return;

        if(isWaiting)
        {
            if(Time.time - lastTimeBeginWait > waitingTimeCheckPoints[targetIndex])
            {
                isWaiting = false;
                targetIndex = (targetIndex + 1) % checkPoints.Length;
                dir = (checkPoints[targetIndex] - (Vector2)transform.position).normalized;
            }
        }
        else
        {
            transform.Translate(dir * (speeds[targetIndex] * Time.deltaTime));
            if (checkPoints[targetIndex].SqrDistance(transform.position) <= 4f * speeds[targetIndex] * speeds[targetIndex] * Time.deltaTime * Time.deltaTime)
            {
                transform.position = checkPoints[targetIndex];
                isWaiting = true;
                lastTimeBeginWait = Time.time;
            }

            if(Mathf.Abs(dir.x) >= 1e-6f)
            {
                if(dir.x > 0f)
                {
                    ApplyDetection(offsetHitboxRight, sizeHitboxRight, Vector2.right);
                }
                else
                {
                    ApplyDetection(offsetHitboxLeft, sizeHitboxLeft, Vector2.left);
                }
            }
            if (Mathf.Abs(dir.y) >= 1e-6f)
            {
                if (dir.y > 0f)
                {
                    ApplyDetection(offsetHitboxUp, sizeHitboxUp, Vector2.up);
                }
                else
                {
                    ApplyDetection(offsetHitboxDown, sizeHitboxDown, Vector2.down);
                }
            }

            void ApplyDetection(in Vector2 offset, in Vector2 size, in Vector2 dir)
            {
                UnityEngine.Collider2D[] cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position + offset, size, 0f, charMask);
                foreach (UnityEngine.Collider2D col in cols)
                {
                    if (col.CompareTag("Char"))
                    {
                        PlayerCommon pc = col.GetComponent<ToricObject>().original.GetComponent<PlayerCommon>();
                        if(!charAlreadyTouch.Contains(pc.id))
                        {
                            BoxCollider2D charHitbox = pc.GetComponent<BoxCollider2D>();
                            ToricRaycastHit2D raycast = PhysicsToric.Raycast((Vector2)transform.position + offset, dir, charHitbox.size.x * 1.1f, groundMask);

                            if (raycast.collider != null)
                            {
                                TouchPlayerByEnvironement(pc);
                            }
                        }
                    }
                }
            }

            void TouchPlayerByEnvironement(PlayerCommon pc)
            {
                charAlreadyTouch.Add(pc.id);
                pc.GetComponent<EventController>().OnBeenKillByEnvironnement(gameObject);
            }
        }
    }

    #region Gizmos/OnValidate

    private void Enable()
    {
        enableBehaviour = true;
    }

    private void Disable()
    {
        enableBehaviour = false;
    }

    private void OnDestroy()
    {
        PauseManager.instance.callBackOnPauseEnable -= Disable;
        PauseManager.instance.callBackOnPauseDisable -= Enable;
    }

    private void OnValidate()
    {
        for (int i = 0; i < speeds.Length; i++)
        {
            speeds[i] = Mathf.Max(0f, speeds[i]);
        }
        for (int i = 0; i < waitingTimeCheckPoints.Length; i++)
        {
            waitingTimeCheckPoints[i] = Mathf.Max(0f, waitingTimeCheckPoints[i]);
        }

        detectionHitboxSize = Mathf.Max(0f, detectionHitboxSize);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < checkPoints.Length; i++)
        {
            Circle.GizmosDraw(checkPoints[i], 0.3f);
        }

        if (mainHitbox == null)
            mainHitbox = GetComponent<BoxCollider2D>();
        CalculateDetectionHitbox();
        Gizmos.DrawWireCube((Vector2)transform.position + offsetHitboxUp, sizeHitboxUp);
        Gizmos.DrawWireCube((Vector2)transform.position + offsetHitboxDown, sizeHitboxDown);
        Gizmos.DrawWireCube((Vector2)transform.position + offsetHitboxRight, sizeHitboxRight);
        Gizmos.DrawWireCube((Vector2)transform.position + offsetHitboxLeft, sizeHitboxLeft);
    }

    #endregion
}
