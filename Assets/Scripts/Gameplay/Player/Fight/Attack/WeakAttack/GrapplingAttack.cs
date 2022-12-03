using System;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingAttack : WeakAttack
{
    private LineRenderer lineRendererPrefabs;
    private List<LineRenderer> lstlineRenderers;
    private Movement movement;
    private Rigidbody2D rb;
    private Action callbackEnd;
    private CustomPlayerInput playerInput;
    private bool isSwinging;
    private Vector2 grapDir;
    private float grapLength;
    private float lastTimeGrap = -10f;
    private Vector2 localFloorAttachPos;
    private GameObject goWhereGrapIsAttach;
    private Collider2D colliderWhereGrapIsAttach;
    private Vector2[] toricIntersPoints;
    private float lastTimeBombSpawn = -10f;
    private bool doJump;

    private GameObject physicSimulateClone;
    private SpringJoint2D physicSimulateCloneSpringJoint;
    private Rigidbody2D physicSimulateCloneRb;

    [SerializeField] private float grapRange, circleCastRadius = 0.5f;
    [SerializeField, Tooltip("%age d'élasticité max de la corde")] private float ropeElasticity = 0.2f;
    [SerializeField] private float maxDurationAttach = 5f;
    [SerializeField] private float grapClimbUpSpeed = 2f, grapClimbDownSpeed = 4f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Bomb bombPrefabs;
    [SerializeField] private float timeBetweenBombSpawn = 0.4f;

    [Header("Physics simulation")]
    [SerializeField] private float gravityScaleWhenSwinging = 1f;
    [SerializeField] private float grapMovementForce = 5f, linearDrag = 0.05f;
    [SerializeField] private Component[] componentsToDestroyInPhysicsSimulateClone;

    protected override void Awake()
    {
        base.Awake();
        lineRendererPrefabs = transform.GetChild(1).GetChild(0).GetComponent<LineRenderer>();
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody2D>();
        lstlineRenderers = new List<LineRenderer>()
        {
            lineRendererPrefabs
        };
        playerInput = GetComponent<CustomPlayerInput>();
    }

    protected override void Start()
    {
        base.Start();
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
            BeginSwing();
            this.callbackEnd = callbackEnd;
            return true;
        }
        return false;
    }

    private void BeginSwing()
    {
        isSwinging = true;
        movement.enableBehaviour = false;
        lastTimeGrap = Time.time;

        //mise en place du clone
        physicSimulateClone = Instantiate(gameObject, Vector3.zero, Quaternion.identity, CloneParent.cloneParent);
        physicSimulateClone.DestroyChildren();
        physicSimulateClone.name = "PhysicSimulateClone";
        foreach (Component c in componentsToDestroyInPhysicsSimulateClone)
        {
            Destroy(physicSimulateClone.GetComponent(c.GetType()));
        }

        physicSimulateCloneSpringJoint = physicSimulateClone.GetComponent<SpringJoint2D>();
        physicSimulateCloneRb = physicSimulateClone.GetComponent<Rigidbody2D>();

        physicSimulateCloneSpringJoint.enabled = true;
        physicSimulateCloneSpringJoint.connectedBody = goWhereGrapIsAttach.GetComponentInChildren<Rigidbody2D>();
        physicSimulateCloneSpringJoint.connectedAnchor = localFloorAttachPos;
        physicSimulateCloneSpringJoint.distance = grapLength;
        physicSimulateCloneRb.gravityScale = gravityScaleWhenSwinging;

        Vector2 attachPos = (Vector2)goWhereGrapIsAttach.transform.position + localFloorAttachPos;
        physicSimulateClone.transform.position = attachPos - grapDir * grapLength;
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
            physicSimulateCloneRb.AddForce(Vector2.left * (Time.fixedDeltaTime * grapMovementForce), ForceMode2D.Force);
        }

        if (playerInput.rightPressed)
        {
            physicSimulateCloneRb.AddForce(Vector2.right * (Time.fixedDeltaTime * grapMovementForce), ForceMode2D.Force);
        }

        physicSimulateCloneRb.AddForce(physicSimulateCloneRb.velocity * (-linearDrag * Time.fixedDeltaTime), ForceMode2D.Force);
    }

    protected override void Update()
    {
        base.Update();

        if (!isSwinging)
            return;

        rb.position = physicSimulateClone.transform.position;

        if(!RecalculateInterPoint())
        {
            EndAttack();
            return;
        }

        UpdateLinesRenderer();

        if(playerInput.downPressed)
        {
            physicSimulateCloneSpringJoint.distance += grapClimbDownSpeed * Time.deltaTime;
        }
        if(playerInput.upPressed)
        {
            physicSimulateCloneSpringJoint.distance -= grapClimbUpSpeed * Time.deltaTime;
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
            return;
        }

        if (Time.time - lastTimeGrap > maxDurationAttach || !playerInput.attackWeakPressed)
        {
            EndAttack();
            return;
        }

        bool RecalculateInterPoint()
        {
            grapDir = ((Vector2)goWhereGrapIsAttach.transform.position + localFloorAttachPos - (Vector2)physicSimulateClone.transform.position).normalized;
            RaycastHit2D raycast = PhysicsToric.Raycast(transform.position, grapDir, grapLength * (1f + ropeElasticity), groundMask, out toricIntersPoints);
            if(raycast.collider == null || raycast.collider != colliderWhereGrapIsAttach)
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
            isSwinging = false;
            RemoveLineRenderer();
            toricIntersPoints = null;
            cooldown.Reset();
            callbackEnd.Invoke();
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
        localFloorAttachPos = raycast.point - (Vector2)goWhereGrapIsAttach.transform.position;
        grapLength = raycast.distance;
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
        ropeElasticity = Mathf.Max(0f, ropeElasticity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw(transform.position, grapRange);
        Circle.GizmosDraw((Vector2)transform.position + grapRange * Vector2.up, circleCastRadius);

        if(Application.isPlaying && false)
        {
            grapDir = movement.GetCurrentDirection();
            if(CalculateAttachPoint())
            {
                Gizmos.color = Color.red;
                Circle.GizmosDraw((Vector2)goWhereGrapIsAttach.transform.position + localFloorAttachPos, 0.3f);
            }
        }
    }
}
