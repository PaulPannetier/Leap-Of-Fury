using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Collision2D;

public class GrapplingAttack : WeakAttack
{
    private LineRenderer lineRendererPrefabs;
    private List<LineRenderer> lstlineRenderers;
    private CharacterController movement;
    private Rigidbody2D rb;
    private SpringJoint2D springJoint;
    private CustomPlayerInput playerInput;
    private Rigidbody2D rbAttachPoint;
    private bool isSwinging;
    private Vector2 grapDir;
    private Vector2 attachPoint;
    private UnityEngine.Collider2D colliderWhereGrapIsAttach;
    private float grapLength;
    private Vector2[] lineRendererPoints;
    private float lastTimeBombSpawn = -10f;
    private bool doJump, doDash;
    private float lastTimeGrap = -10f;
    private Action callbackEnableThisAttack;
    private bool isWaiting; //true => on viens de ce tp, on attend 2 frame avant de reprendre les updates
    private bool endAttack;
    private LayerMask groundMask;

#if UNITY_EDITOR

    public bool drawGizmos = true;

#endif

    [SerializeField] private float grapRange, circleCastRadius = 0.5f;
    [SerializeField] private float maxRopeLength;
    [SerializeField] private float minRopeLength;
    [SerializeField] private float maxDurationAttach = 5f;
    [SerializeField] private float grapClimbUpSpeed = 2f, grapClimbDownSpeed = 4f;
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
        movement = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody2D>();
        springJoint = GetComponent<SpringJoint2D>();
        lstlineRenderers = new List<LineRenderer>()
        { 
            lineRendererPrefabs
        };
        playerInput = GetComponent<CustomPlayerInput>();
    }

    protected override void Start()
    {
        base.Start();
        GetComponent<ToricObject>().onTeleportCallback += OnTeleportByToricObjectScript;
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
    }

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
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
            base.Launch(callbackEnableOtherAttack, callbackEnableThisAttack);
            return true;
        }

        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
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
            endAttack = true;
        }

        if(doDash)
        {
            movement.RequestDash(movement.GetCurrentDirection(true));
            doDash= false;
            endAttack = true;
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
        rb.AddForce(Physics2D.gravity * (gravityScaleWhenSwinging * Time.fixedDeltaTime), ForceMode2D.Force);
    }

    protected override void Update()
    {
        base.Update();

        if (!isSwinging)
            return;

        if(!isWaiting)
        {
            if (!RecalculateInterPoint())
            {
                EndAttack();
                return;
            }

            UpdateLinesRenderer();

            if (playerInput.downPressed)
            {
                grapLength = Mathf.Min(grapLength + grapClimbDownSpeed * Time.deltaTime, maxRopeLength);
                springJoint.distance = grapLength;
            }

            if (playerInput.upPressed)
            {
                grapLength = Mathf.Max(grapLength - grapClimbUpSpeed * Time.deltaTime, minRopeLength);
                springJoint.distance = grapLength;
            }

            if (Time.time - lastTimeBombSpawn > timeBetweenBombSpawn)
            {
                lastTimeBombSpawn = Time.time;
                Bomb bomb = Instantiate(bombPrefabs, transform.position, Quaternion.identity, CloneParent.cloneParent);
                bomb.Lauch(this);
            }
        }

        if (playerInput.jumpPressedDown)
        {
            doJump = true;
            return;
        }

        if (playerInput.dashPressedDown)
        {
            doDash = true;
            return;
        }

        if (movement.isGrounded)
        {
            EndAttack();
            return;
        }

        if (playerInput.attackWeakPressedUp || Time.time - lastTimeGrap > maxDurationAttach)
        {
            EndAttack();
            return;
        }

        if(endAttack)
        {
            EndAttack();
            return;
        }

        void UpdateLinesRenderer()
        {
            int nbLineRenderer = lineRendererPoints.Length + 1;
            while (lstlineRenderers.Count < nbLineRenderer)
            {
                lstlineRenderers.Add(Instantiate(lineRendererPrefabs, transform.GetChild(1)));
            }
            while (lstlineRenderers.Count > nbLineRenderer)
            {
                Destroy(transform.GetChild(1).GetChild(transform.GetChild(1).childCount - 1).gameObject);
                lstlineRenderers.RemoveAt(lstlineRenderers.Count - 1);
            }

            Vector2 beg = PhysicsToric.GetPointInsideBounds(transform.position), end;
            for (int i = 0; i < nbLineRenderer; i++)
            {
                end = i != nbLineRenderer - 1 ? lineRendererPoints[i] : PhysicsToric.GetPointInsideBounds(attachPoint);
                lstlineRenderers[i].positionCount = 2;
                lstlineRenderers[i].SetPositions(new Vector3[2] { beg, end });
                Hitbox mapHitbox = new Hitbox(Vector2.zero, LevelMapData.currentMap.mapSize);
                if(i != nbLineRenderer - 1)
                {
                    while (mapHitbox.Contains(end))
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
            isSwinging = doJump = doDash = endAttack = false;
            RemoveLineRenderer();
            lineRendererPoints = null;
            Destroy(rbAttachPoint.gameObject);
            cooldown.Reset();
            callbackEnableThisAttack.Invoke();
        }
    }

    private void OnTeleportByToricObjectScript(Vector2 newPos, Vector2 oldPos)
    {
        if (!isSwinging)
            return;

        StopCoroutine(nameof(OnTeleportByToricObjectCorout));
        StartCoroutine(OnTeleportByToricObjectCorout(newPos, oldPos));
    }

    private IEnumerator OnTeleportByToricObjectCorout(Vector2 newPos, Vector2 oldPos)
    {
        isWaiting = true;
        yield return null;
        yield return null;

        isWaiting = false;
        Vector2 shift = newPos - oldPos;
        rbAttachPoint.transform.position = (Vector2)rbAttachPoint.transform.position + shift;
        attachPoint += shift;
        RecalculateInterPoint();
    }

    private bool CalculateAttachPoint()
    {
        grapDir = movement.GetCurrentDirection(true);
        float grapAngle = Useful.AngleHori(Vector2.zero, grapDir);

        if(grapAngle < 45f * Mathf.Deg2Rad || grapAngle > 135f * Mathf.Deg2Rad)
        {
            return false;
        }

        grapDir = Useful.Vector2FromAngle(grapAngle);
        ToricRaycastHit2D raycast = PhysicsToric.CircleCast(transform.position, grapDir, circleCastRadius, grapRange, groundMask, out lineRendererPoints);
        if(raycast.collider == null)
        {
            return false;
        }

        grapLength = raycast.distance;
        attachPoint = (Vector2)transform.position + grapLength * grapDir;
        colliderWhereGrapIsAttach = raycast.collider;

        return true;
    }

    private bool RecalculateInterPoint()
    {
        Vector2 pos = transform.position;
        grapDir = (attachPoint - pos).normalized;
        ToricRaycastHit2D raycast = PhysicsToric.Raycast(pos, grapDir, grapLength * (1f + grapElasticity), groundMask, out lineRendererPoints);
        if (raycast.collider != null && raycast.collider != colliderWhereGrapIsAttach)
        {
            return false;
        }
        return true;
    }

    private void PauseTheGame()
    {
        Debug.Break();
    }

    #region Gizmos / OnValidate

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        grapClimbUpSpeed = Mathf.Max(0f, grapClimbUpSpeed);
        grapClimbDownSpeed = Mathf.Max(0f, grapClimbDownSpeed);
        grapRange = Mathf.Max(0f, grapRange);
        maxDurationAttach = Mathf.Max(0f, maxDurationAttach);
        circleCastRadius = Mathf.Max(0f, circleCastRadius);
        GetComponent<SpringJoint2D>().enabled = false;
        grapElasticity = Mathf.Max(0f, grapElasticity);
        maxRopeLength = Mathf.Max(grapRange, maxRopeLength);
        minRopeLength = Mathf.Max(minRopeLength, 0f);
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
    }

    private void OnDrawGizmosSelected()
    {
        if(!drawGizmos)
            return;

        Gizmos.color = Color.green;
        Circle.GizmosDraw(transform.position, grapRange);
        Circle.GizmosDraw((Vector2)transform.position + grapRange * Vector2.up, circleCastRadius);
    }

#endif

#endregion
}
