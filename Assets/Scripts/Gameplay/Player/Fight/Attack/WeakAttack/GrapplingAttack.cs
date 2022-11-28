using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrapplingAttack : WeakAttack
{
    private LineRenderer lineRendererPrefabs;
    private List<LineRenderer> lstlineRenderers;
    private Movement movement;
    private Action callbackEnd;
    private SpringJoint2D springJoint;
    private CustomPlayerInput playerInput;
    private Rigidbody2D rb;
    private bool isSwinging;
    private Vector2 grapDir;
    private float lastTimeGrap = -10f;
    private Vector2 localFloorAttachPos;
    private GameObject goWhereGrapIsAttach;
    private float gravityScaleBeforeSwinging;//pour remettre la valeur de la gravité apres le swing
    private Vector2[] toricIntersPoints;
    private float lastTimeBombSpawn = -10f;
    private bool doJump;

    [SerializeField] private float grapRange, circleCastRadius = 0.5f, gravityScaleWhenSwinging = 1f;
    [SerializeField] private float maxDurationAttach = 5f;
    [SerializeField] private float grapMovementForce = 5f, linearDrag = 0.05f;
    [SerializeField] private float grapClimbUpSpeed = 2f, grapClimbDownSpeed = 4f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Bomb bombPrefabs;
    [SerializeField] private float timeBetweenBombSpawn = 0.4f;

    protected override void Awake()
    {
        base.Awake();
        lineRendererPrefabs = transform.GetChild(1).GetChild(0).GetComponent<LineRenderer>();
        movement = GetComponent<Movement>();
        lstlineRenderers = new List<LineRenderer>()
        {
            lineRendererPrefabs
        };
        springJoint = GetComponent<SpringJoint2D>();
        playerInput = GetComponent<CustomPlayerInput>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void Start()
    {
        base.Start();
        springJoint.enabled = false;
    }

    public override bool Launch(Action callbackEnd)
    {
        base.Launch(callbackEnd);
        if (!cooldown.isActive)
        {
            callbackEnd.Invoke();
            return false;
        }

        grapDir = movement.GetCurrentDirection();
        if (CalculateAttachPoint())
        {
            isSwinging = true;
            movement.enableBehaviour = false;
            gravityScaleBeforeSwinging = rb.gravityScale;
            rb.gravityScale = gravityScaleWhenSwinging;
            springJoint.enabled = true;
            springJoint.connectedBody = goWhereGrapIsAttach.GetComponentInChildren<Rigidbody2D>();
            springJoint.connectedAnchor = localFloorAttachPos;
            springJoint.distance = ((Vector2)transform.position).Distance((Vector2)goWhereGrapIsAttach.transform.position + localFloorAttachPos);
            lastTimeGrap = Time.time;
            cooldown.Reset();
            this.callbackEnd = callbackEnd;
            return true;
        }
        return false;
    }

    protected override void FixedUpdate()
    {
        if (doJump)
        {
            movement.RequestWallJump(playerInput.x < 0f);
            doJump = false;
        }

        if (!isSwinging)
            return;

        if (playerInput.leftPressed)
        {
            rb.AddForce(Vector2.left * (Time.fixedDeltaTime * grapMovementForce), ForceMode2D.Force);
        }

        if (playerInput.rightPressed)
        {
            rb.AddForce(Vector2.right * (Time.fixedDeltaTime * grapMovementForce), ForceMode2D.Force);
        }
        
        rb.AddForce(rb.velocity * (-linearDrag * Time.fixedDeltaTime), ForceMode2D.Force);
    }

    protected override void Update()
    {
        base.Update();

        if (!isSwinging)
            return;

        UpdateLinesRenderer();

        if(playerInput.downPressed)
        {
            springJoint.distance += grapClimbDownSpeed * Time.deltaTime;
        }
        if(playerInput.upPressed)
        {
            springJoint.distance -= grapClimbUpSpeed * Time.deltaTime;
        }

        if(lastTimeBombSpawn - Time.time > timeBetweenBombSpawn)
        {
            lastTimeBombSpawn = Time.time;
            Bomb bomb = Instantiate(bombPrefabs, transform.position, Quaternion.identity, CloneParent.cloneParent);
            bomb.Lauch(this);
        }

        if(playerInput.jumpPressedDown)
        {
            doJump = true;
            EndAttack();
        }

        if (Time.time - lastTimeGrap > maxDurationAttach || !playerInput.attackWeakPressed)
        {
            EndAttack();
        }

        void UpdateLinesRenderer()
        {
            RecalculateInterPoints();

            int nbLineRenderer = toricIntersPoints.Length + 1;
            while (lstlineRenderers.Count < nbLineRenderer)
            {
                lstlineRenderers.Add(Instantiate(lineRendererPrefabs, transform.GetChild(1)));
            }
            while (lstlineRenderers.Count > nbLineRenderer)
            {
                Destroy(transform.GetChild(1).GetChild(transform.GetChild(1).childCount - 1));
                lstlineRenderers.RemoveAt(lstlineRenderers.Count - 1);
            }
            Vector2 beg = transform.position, end;
            for (int i = 0; i < nbLineRenderer; i++)
            {
                end = i != nbLineRenderer - 1 ? toricIntersPoints[i] : (Vector2)goWhereGrapIsAttach.transform.position + localFloorAttachPos;
                lstlineRenderers[i].positionCount = 2;
                lstlineRenderers[i].SetPositions(new Vector3[2] { beg, end });
                if(i != nbLineRenderer - 1)
                {
                    while (PhysicsToric.cameraHitbox.Contains(end))
                    {
                        end += new Vector2(0.01f * (end.x >= 0f ? 1f : -1), 0.01f * (end.y >= 0f ? 1f : -1f));
                    }
                }
                beg = PhysicsToric.GetPointInsideBounds(end);
            }
        }

        void RecalculateInterPoints()
        {
            Vector2[] tmpInter = (Vector2[])toricIntersPoints.Clone();


        }

        void RemoveLineRenderer()
        {
            while (lstlineRenderers.Count > 1)
            {
                Destroy(transform.GetChild(1).GetChild(transform.GetChild(1).childCount - 1).gameObject);
                lstlineRenderers.RemoveAt(lstlineRenderers.Count - 1);
            }
            lstlineRenderers[0].positionCount = 0;
            lstlineRenderers[0].SetPositions(new Vector3[0]);
        }

        void EndAttack()
        {
            rb.gravityScale = gravityScaleBeforeSwinging;
            movement.enableBehaviour = movement.enableInput = true;
            springJoint.enabled = false;
            isSwinging = false;
            RemoveLineRenderer();
            callbackEnd.Invoke();
            return;
        }
    }

    private bool CalculateAttachPoint()
    {
        RaycastHit2D raycast = PhysicsToric.CircleCast(transform.position, grapDir, circleCastRadius, grapRange, groundMask, out toricIntersPoints);
        if(raycast.collider == null)
        {
            return false;
        }

        goWhereGrapIsAttach = raycast.collider.gameObject;
        localFloorAttachPos = raycast.point - (goWhereGrapIsAttach.transform.parent != null ? (Vector2)goWhereGrapIsAttach.transform.parent.position : Vector2.zero);
        return true;
    }

    private void OnValidate()
    {
        grapClimbUpSpeed = Mathf.Max(0f, grapClimbUpSpeed);
        grapClimbDownSpeed = Mathf.Max(0f, grapClimbDownSpeed);
        grapRange = Mathf.Max(0f, grapRange);
        maxDurationAttach = Mathf.Max(0f, maxDurationAttach);
        circleCastRadius = Mathf.Max(0f, circleCastRadius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw(transform.position, grapRange);
        Circle.GizmosDraw((Vector2)transform.position + grapRange * Vector2.up, circleCastRadius);
    }
}
