using System;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingAttack : WeakAttack
{
    private LineRenderer lineRendererPrefabs;
    private List<LineRenderer> lstlineRenderers;
    private Movement movement;
    private Rigidbody2D rb;
    private SpringJoint2D springJoint;
    private CustomPlayerInput playerInput;
    private Rigidbody2D rbAttachPoint;
    private bool isSwinging;
    private Vector2 grapDir;
    private Vector2 attachPoint;
    private Collider2D colliderWhereGrapIsAttach;
    private float grapLength;
    private Vector2[] toricIntersPoints;
    private float lastTimeBombSpawn = -10f;
    private bool doJump, doDash;
    private float lastTimeGrap = -10f;
    private Action callbackEnableThisAttack;

    //to rm
    private Line lineToDraw;

    [SerializeField] private float grapRange, circleCastRadius = 0.5f;
    [SerializeField] private float maxRopeLength;
    [SerializeField] private float maxDurationAttach = 5f;
    [SerializeField] private float grapClimbUpSpeed = 2f, grapClimbDownSpeed = 4f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Bomb bombPrefabs;
    [SerializeField] private float timeBetweenBombSpawn = 0.4f;

    [Header("Physics simulation")]
    [SerializeField] private GameObject attackPointPrefaps;
    [SerializeField] private float grapElasticity = 0.2f;
    [SerializeField] private float gravityScaleWhenSwinging = 1f;
    [SerializeField] private float grapMovementForce = 5f, linearDrag = 0.05f;

    protected override void Awake()
    {
        base.Awake();
        lineRendererPrefabs = transform.GetChild(1).GetChild(0).GetComponent<LineRenderer>();
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody2D>();
        springJoint = GetComponent<SpringJoint2D>();
        lstlineRenderers = new List<LineRenderer>()
        { 
            lineRendererPrefabs
        };
        playerInput = GetComponent<CustomPlayerInput>();
    }

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        base.Launch(callbackEnableOtherAttack, callbackEnableThisAttack);
        if (!cooldown.isActive)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        if (CalculateAttachPoint())
        {
            BeginSwing();
            callbackEnableOtherAttack.Invoke();
            this.callbackEnableThisAttack = callbackEnableThisAttack;
            return true;
        }
        return false;
    }

    private void BeginSwing()
    {
        isSwinging = true;
        movement.enableBehaviour = false;

        GameObject attachPointGO = Instantiate(attackPointPrefaps, attachPoint, Quaternion.identity, CloneParent.cloneParent);
        rbAttachPoint = attachPointGO.GetComponent<Rigidbody2D>();

        springJoint.enabled = true;
        springJoint.connectedBody = rbAttachPoint;
        springJoint.distance = grapLength;

        lastTimeGrap = Time.time;
    }

    protected override void FixedUpdate()
    {
        if (doJump)
        {
            movement.RequestWallJump(playerInput.x < 0f);
            doJump = false;
        }

        if(doDash)
        {
            movement.RequestDash(movement.GetCurrentDirection(true));
            doDash= false;
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
        rb.AddForce(Physics2D.gravity * (gravityScaleWhenSwinging * Time.deltaTime), ForceMode2D.Force);
    }

    protected override void Update()
    {
        base.Update();

        if (!isSwinging)
            return;

        if(!RecalculateInterPoint())
        {
            EndAttack();
            return;
        }

        UpdateLinesRenderer();

        if(playerInput.downPressed)
        {
            grapLength = Mathf.Min(grapLength + grapClimbDownSpeed * Time.deltaTime, maxRopeLength);
            springJoint.distance = grapLength;
        }

        if(playerInput.upPressed)
        {
            grapLength = Mathf.Max(grapLength - grapClimbDownSpeed * Time.deltaTime, 0f);
            springJoint.distance = grapLength;
        }

        if(Time.time - lastTimeBombSpawn > timeBetweenBombSpawn)
        {
            lastTimeBombSpawn = Time.time;
            Bomb bomb = Instantiate(bombPrefabs, transform.position, Quaternion.identity, CloneParent.cloneParent);
            bomb.Lauch(this);
        }

        if(playerInput.jumpPressedDown)
        {
            doJump = true;
            EndAttack();
            return;
        }

        if(playerInput.dashPressedDown)
        {
            doDash = true;
            EndAttack();
            return;
        }

        if (playerInput.attackWeakPressedUp || Time.time - lastTimeGrap > maxDurationAttach)
        {
            EndAttack();
            return;
        }

        bool RecalculateInterPoint()
        {
            Vector2 pos = transform.position;
            grapDir = (attachPoint - pos).normalized;
            lineToDraw = new Line(pos, pos + grapDir * grapLength * (1f + grapElasticity));
            RaycastHit2D raycast = PhysicsToric.Raycast(pos, grapDir, grapLength * (1f + grapElasticity), groundMask, out toricIntersPoints);
            if(raycast.collider != null && raycast.collider != colliderWhereGrapIsAttach)
            {
                return false;
            }
            return true;
        }

        void UpdateLinesRenderer()
        {
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
                end = i != nbLineRenderer - 1 ? toricIntersPoints[i] : attachPoint;
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
            movement.enableBehaviour = movement.enableInput = true;
            isSwinging = doJump = doDash = false;
            RemoveLineRenderer();
            toricIntersPoints = null;
            Destroy(rbAttachPoint.gameObject);
            cooldown.Reset();
            callbackEnableThisAttack.Invoke();
        }
    }

    private bool CalculateAttachPoint()
    {
        grapDir = movement.GetCurrentDirection(true);
        RaycastHit2D raycast = PhysicsToric.CircleCast(transform.position, grapDir, circleCastRadius, grapRange, groundMask, out toricIntersPoints);
        if(raycast.collider == null)
        {
            return false;
        }

        grapLength = raycast.distance;
        attachPoint = raycast.point;
        colliderWhereGrapIsAttach = raycast.collider;

        return true;
    }

    private void OnValidate()
    {
        grapClimbUpSpeed = Mathf.Max(0f, grapClimbUpSpeed);
        grapClimbDownSpeed = Mathf.Max(0f, grapClimbDownSpeed);
        grapRange = Mathf.Max(0f, grapRange);
        maxDurationAttach = Mathf.Max(0f, maxDurationAttach);
        circleCastRadius = Mathf.Max(0f, circleCastRadius);
        GetComponent<SpringJoint2D>().enabled = false;
        grapElasticity = Mathf.Max(0f, grapElasticity);
        maxRopeLength = Mathf.Max(grapRange, maxRopeLength);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw(transform.position, grapRange);
        Circle.GizmosDraw((Vector2)transform.position + grapRange * Vector2.up, circleCastRadius);

        if(Application.isPlaying)
        {
            if(lineToDraw != null)
                Gizmos.DrawLine(lineToDraw.A, lineToDraw.B);
            /*
            if(CalculateAttachPoint())
            {
                Gizmos.color = Color.red;
                Circle.GizmosDraw(attachPoint, circleCastRadius);
                Gizmos.DrawLine(transform.position, attachPoint);
            }
            */
        }
    }
}
