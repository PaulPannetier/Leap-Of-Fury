using System;
using UnityEngine;
using Collision2D;
using PathFinding;
using Collider2D = UnityEngine.Collider2D;
using static Boomerang;

public class BoomerangAttack : WeakAttack
{
    private Movement movement;
    private Boomerang currentBoomerang;

    [SerializeField] private GameObject boomerangPrefab;
    [SerializeField] private float distanceToInstantiate = 0.4f;
    [SerializeField] private AnimationCurve speedCurvePhase1, accelerationCurvePhase2;
    [SerializeField] private float maxSpeedPhase1, maxSpeedPhase2, durationPhase1, accelerationDurationPhase2;
    [SerializeField] private float recuperationRange;
    [SerializeField] private float rotationSpeed = 120f;

#if UNITY_EDITOR

    [SerializeField] private bool drawGizmos = true;

#endif

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<Movement>();
    }


    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if (!cooldown.isActive)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        Vector2 dir = movement.GetCurrentDirection(true);
        Vector2 pos = PhysicsToric.GetPointInsideBounds((Vector2)transform.position + dir * distanceToInstantiate);
        currentBoomerang = Instantiate(boomerangPrefab, pos, Quaternion.identity, CloneParent.cloneParent).GetComponent<Boomerang>();

        currentBoomerang.Launch(CreateLaunchData(dir));

        cooldown.Reset();

        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
        return true;
    }

    private BoomerangLaunchData CreateLaunchData(in Vector2 dir)
    {
        return new BoomerangLaunchData(dir, speedCurvePhase1, accelerationCurvePhase2, maxSpeedPhase1, durationPhase1,
            accelerationDurationPhase2, this, maxSpeedPhase2, recuperationRange, rotationSpeed * Mathf.Deg2Rad);
    }

    public void GetBack()
    {
        currentBoomerang = null;
    }

#if UNITY_EDITOR

    protected void OnValidate()
    {
        durationPhase1 = Mathf.Max(0f, durationPhase1);
        accelerationDurationPhase2 = Mathf.Max(0f, accelerationDurationPhase2);
        maxSpeedPhase1 = Mathf.Max(0f, maxSpeedPhase1);
        maxSpeedPhase2 = Mathf.Max(0f, maxSpeedPhase2);
        distanceToInstantiate = Mathf.Max(0f, distanceToInstantiate);
        recuperationRange = Mathf.Max(0f, recuperationRange);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
    }

    [SerializeField] private bool testPathNoDiagFinding;
    [SerializeField] private bool testPathFinding;
    [SerializeField] private bool useSplinePathFinding;
    [SerializeField] private bool useExtraSmoothness;
    [SerializeField] private BezierUtility.SplineType splineType = BezierUtility.SplineType.Catmulrom;
    [SerializeField] private Vector2 start;
    [SerializeField, Range(0f, 1f)] private float tension;
    [SerializeField, Range(0f, 1f)] private float x;
    Path testPathNoDiag, testPath;
    PathFinderToric.SplinePath splinePathNoDiag, splinePath;

    private void OnDrawGizmosSelected()
    {
        Vector2 end = PhysicsToric.GetPointInsideBounds(Useful.mainCamera.ScreenToWorldPoint(InputManager.mousePosition));
        Gizmos.color = Color.green;
        Circle.GizmosDraw(start, 0.3f);
        Circle.GizmosDraw(end, 0.3f);

        if (testPathFinding || testPathNoDiagFinding)
        {
            Map map = LevelMapData.currentMap.GetPathfindingMap();
            MapPoint startMP = LevelMapData.currentMap.GetMapPointAtPosition(start);
            MapPoint endMP = LevelMapData.currentMap.GetMapPointAtPosition(end);
            testPathNoDiag = testPath = null;
            splinePathNoDiag = splinePath = null;

            if (testPathNoDiagFinding)
            {
                if(useSplinePathFinding)
                {
                    splinePathNoDiag = PathFinderToric.FindBestCurve(map, startMP, endMP, LevelMapData.currentMap.GetPositionOfMapPoint, false, splineType, useExtraSmoothness, tension);
                    if(splinePathNoDiag != null)
                    {
                        Gizmos.color = Color.green;
                        DrawPoints(splinePathNoDiag.EvaluateDistance(1000));
                    }
                }
                else
                {
                    testPathNoDiag = PathFinderToric.FindBestPath(map, startMP, endMP, false);
                    Gizmos.color = Color.green;
                    DrawPath(testPathNoDiag);
                }
            }

            if (testPathFinding)
            {
                if (useSplinePathFinding)
                {
                    splinePath = PathFinderToric.FindBestCurve(map, startMP, endMP, LevelMapData.currentMap.GetPositionOfMapPoint, true, splineType, useExtraSmoothness, tension);
                    if (splinePath != null)
                    {
                        Gizmos.color = Color.green;
                        DrawPoints(splinePath.EvaluateDistance(1000));
                    }
                }
                else
                {
                    testPath = PathFinderToric.FindBestPath(map, startMP, endMP, true);
                    Gizmos.color = Color.red;
                    DrawPath(testPath);
                }
            }

            if(useSplinePathFinding && (splinePathNoDiag != null || splinePath != null))
            {
                if(splinePathNoDiag != null)
                {
                    Vector2 p = splinePathNoDiag.EvaluateDistance(x);
                    Gizmos.color = Color.red;
                    Circle.GizmosDraw(p, 0.3f);
                }
                if (splinePath != null)
                {
                    Vector2 p = splinePath.EvaluateDistance(x);
                    Gizmos.color = Color.red;
                    Circle.GizmosDraw(p, 0.3f);
                }
            }

            void DrawPath(Path p)
            {
                Vector2 beg = LevelMapData.currentMap.GetPositionOfMapPoint(p.path[0]);

                for (int i = 0; i < p.path.Length; i++)
                {
                    Vector2 end = LevelMapData.currentMap.GetPositionOfMapPoint(p.path[i]);
                    PhysicsToric.GizmosDrawRaycast(beg, end);
                    beg = end;
                }
            }

            void DrawPoints(Vector2[] points)
            {
                Vector2 beg = points[0];
                for (int i = 1; i < points.Length; i++)
                {
                    Color c = Gizmos.color;
                    Gizmos.color = Color.red;
                    Circle.GizmosDraw(PhysicsToric.GetPointInsideBounds(points[i]), 0.05f);
                    Gizmos.color = c;
                    PhysicsToric.GizmosDrawRaycast(beg, points[i]);
                    beg = points[i];
                }
            }
        }

        if (!drawGizmos)
            return;

        Gizmos.color = Color.green;
        Circle.GizmosDraw(transform.position, recuperationRange);
        Gizmos.color = Color.black;
        Circle.GizmosDraw(transform.position, distanceToInstantiate);
    }

#endif
}
