#if UNITY_EDITOR

using PathFinding;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;
using UnityEngine;

public class PathFindingToricTest : MonoBehaviour
{
    [SerializeField] private bool testPathNoDiagFinding;
    [SerializeField] private bool testPathFinding;
    [SerializeField] private bool useSplinePathFinding;
    [SerializeField] private PathFinderToric.SmoothnessMode smoothnessMode;
    [SerializeField] private BezierUtility.SplineType splineType = BezierUtility.SplineType.Catmulrom;
    [SerializeField] private Vector2 start;
    [SerializeField, Range(0f, 1f)] private float tension;
    [SerializeField, Range(0f, 1f)] private float x;
    [SerializeField] private InputKey freezeEndKey = InputKey.Space;
    private Path testPathNoDiag, testPath;
    private PathFinderToric.SplinePath splinePathNoDiag, splinePath;
    private bool freezeEnd;
    private Vector2 end;

    private void Update()
    {
        if(InputManager.GetKeyDown(freezeEndKey))
        {
            freezeEnd = !freezeEnd;
            end = PhysicsToric.GetPointInsideBounds(Useful.mainCamera.ScreenToWorldPoint(InputManager.mousePosition));
        }
    }

    private void OnDrawGizmosSelected()
    {
        if(!freezeEnd)
            end = PhysicsToric.GetPointInsideBounds(Useful.mainCamera.ScreenToWorldPoint(InputManager.mousePosition));

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
                if (useSplinePathFinding)
                {
                    splinePathNoDiag = PathFinderToric.FindBestCurve(map, startMP, endMP, LevelMapData.currentMap.GetPositionOfMapPoint, false, splineType, smoothnessMode, tension);
                    if (splinePathNoDiag != null)
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
                    splinePath = PathFinderToric.FindBestCurve(map, startMP, endMP, LevelMapData.currentMap.GetPositionOfMapPoint, true, splineType, smoothnessMode, tension);
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

            if (useSplinePathFinding && (splinePathNoDiag != null || splinePath != null))
            {
                if (splinePathNoDiag != null)
                {
                    Vector2 p = splinePathNoDiag.EvaluateDistance(x);
                    Vector2 v = splinePathNoDiag.Velocity(x);
                    Gizmos.color = Color.red;
                    Circle.GizmosDraw(p, 0.3f);
                    Useful.GizmoDrawVector(p, v);
                }
                if (splinePath != null)
                {
                    Vector2 p = splinePath.EvaluateDistance(x);
                    Vector2 v = splinePath.Velocity(x);
                    Gizmos.color = Color.red;
                    Circle.GizmosDraw(p, 0.3f);
                    Useful.GizmoDrawVector(p, v);
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
    }

}

#endif