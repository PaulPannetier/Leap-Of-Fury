using System;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingAttack : WeakAttack
{
    private LineRenderer lineRendererPrefabs;
    private List<LineRenderer> lstlineRenderers;
    private bool isLaunch = false, isGrapInAir = false;
    private Vector2 grapDir;
    private Movement movement;
    private Action callbackEnd;
    private SpringJoint2D springJoint;
    private CustomPlayerInput playerInput;
    private Rigidbody2D rb;

    //Update grabInAir = true
    private float currentGrabDistance, currentGrapSpeed;
    private bool isGrabInAirInit = false;

    //Update isGrapInAir = false
    private Vector2 localFloorAttachPos;
    private GameObject goWhereGrapIsAttach;
    private float lastTimeAttach = -10f;
    private float gravityScaleBeforeSwinging;

    [SerializeField] private float grapRange, gravityScaleWhenSwinging = 1f;
    [SerializeField] private float maxDurationAttach = 5f;
    [SerializeField] private float grapSpeed;
    [SerializeField, Tooltip("en %age Vmax/sec")] private float grapSpeedLerp;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Bomb bombPrefabs;

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

        isLaunch = isGrapInAir = true;
        currentGrabDistance = currentGrapSpeed = 0f;
        grapDir = movement.GetCurrentDirection();
        cooldown.Reset();
        this.callbackEnd = callbackEnd;
        return true;
    }

    protected override void Update()
    {
        base.Update();
        if (!isLaunch)
            return;
        
        if(isGrapInAir)
        {
            currentGrabDistance += currentGrapSpeed * Time.deltaTime;
            currentGrapSpeed = Mathf.MoveTowards(currentGrapSpeed, grapSpeed, grapSpeedLerp * grapSpeed * Time.deltaTime);
            if(currentGrabDistance >= grapRange || !playerInput.attackWeakPressed)
            {
                gravityScaleBeforeSwinging = -1f;
                EndAttack();
                return;
            }

            RaycastHit2D raycast = PhysicsToric.Raycast(transform.position, grapDir, currentGrabDistance, groundMask, out Vector2[] intPoints);
            if(raycast.collider != null)
            {
                isGrapInAir = false;
                goWhereGrapIsAttach = raycast.collider.gameObject;
                localFloorAttachPos = raycast.point - (goWhereGrapIsAttach.transform.parent != null ? (Vector2)goWhereGrapIsAttach.transform.parent.position : Vector2.zero);
            }

            UpdateLinesRenderer();

            void UpdateLinesRenderer()
            {
                int nbLineRenderer = intPoints.Length + 1;
                while(lstlineRenderers.Count < nbLineRenderer)
                {
                    lstlineRenderers.Add(Instantiate(lineRendererPrefabs, transform.GetChild(1)));
                }
                while (lstlineRenderers.Count > nbLineRenderer)
                {
                    Destroy(transform.GetChild(1).GetChild(transform.GetChild(1).childCount - 1));
                    lstlineRenderers.RemoveAt(lstlineRenderers.Count- 1);
                }
                Vector2 beg = transform.position, end;
                float totalDist = 0f;
                for (int i = 0; i < nbLineRenderer; i++)
                {
                    end = i == nbLineRenderer - 1 ? (raycast.collider != null ? raycast.point : beg + grapDir * (currentGrabDistance - totalDist)) : intPoints[i];
                    lstlineRenderers[i].positionCount = 2;
                    lstlineRenderers[i].SetPositions(new Vector3[2] { beg, end });
                    totalDist += beg.Distance(end);
                    beg = PhysicsToric.GetPointInsideBounds(end);
                }
            }
        }
        else//grabAttached
        {
            if(!isGrabInAirInit)
            {
                movement.enableBehaviour = false;
                gravityScaleBeforeSwinging = rb.gravityScale;
                rb.gravityScale = gravityScaleWhenSwinging;
                isGrabInAirInit = true;
                springJoint.enabled = true;
                springJoint.connectedBody = goWhereGrapIsAttach.GetComponentInChildren<Rigidbody2D>();
                springJoint.connectedAnchor = localFloorAttachPos;
                springJoint.distance = ((Vector2)transform.position).Distance((Vector2)goWhereGrapIsAttach.transform.position + localFloorAttachPos);
                lastTimeAttach = Time.time;
            }

            if(Time.time - lastTimeAttach > maxDurationAttach || !playerInput.attackWeakPressed)
            {
                EndAttack();
            }
        }

        void RemoveLineRenderer()
        {
            while (lstlineRenderers.Count > 1)
            {
                Destroy(transform.GetChild(1).GetChild(transform.GetChild(1).childCount - 1));
                lstlineRenderers.RemoveAt(lstlineRenderers.Count - 1);
            }
            lstlineRenderers[0].positionCount = 0;
            lstlineRenderers[0].SetPositions(new Vector3[0]);
        }

        void EndAttack()
        {
            rb.gravityScale = gravityScaleBeforeSwinging < 0f ? rb.gravityScale : gravityScaleBeforeSwinging;
            movement.enableBehaviour = movement.enableInput = true;
            isLaunch = isGrabInAirInit = false;
            springJoint.enabled = false;
            RemoveLineRenderer();
            callbackEnd.Invoke();
            return;
        }
    }

    private void OnValidate()
    {
        grapSpeed = Mathf.Max(0f, grapSpeed);
        grapSpeedLerp = Mathf.Max(0f, grapSpeedLerp);
        grapRange = Mathf.Max(0f, grapRange);
        maxDurationAttach = Mathf.Max(0f, maxDurationAttach);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw(transform.position, grapRange);
    }
}
