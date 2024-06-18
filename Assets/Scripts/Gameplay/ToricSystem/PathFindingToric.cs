using PathFinding;
using PathFinding.Graph;
using System.Collections.Generic;
using System;
using UnityEngine;
using static BezierUtility;

public static class PathFinderToric
{
    public enum SmoothnessMode
    {
        None,
        ExtraSmoothness,
        Smoothnesspp
    }

    public static Path FindBestPath(Map map, MapPoint start, MapPoint end, bool allowDiagonal = false)
    {
        return new AStartToric(map, allowDiagonal).CalculateBestPath(start, end);
    }

    public static SplinePath FindBestCurve(Map map, MapPoint start, MapPoint end, Func<MapPoint, Vector2> convertMapPointToWorldPosition, bool allowDiagonal = false, SplineType splineType = SplineType.Catmulrom, SmoothnessMode smoothnessMode = SmoothnessMode.None)
    {
        return FindBestCurve(map, start, end, convertMapPointToWorldPosition, allowDiagonal, splineType, smoothnessMode, -1f);
    }

    public static SplinePath FindBestCurve(Map map, MapPoint start, MapPoint end, Func<MapPoint, Vector2> convertMapPointToWorldPosition, bool allowDiagonal = false, SplineType splineType = SplineType.Catmulrom, SmoothnessMode smoothnessMode = SmoothnessMode.None, float tension = 1f)
    {
#if UNITY_EDITOR || ADVANCE_DEBUG
        if (tension > 0f && splineType != SplineType.Cardinal)
        {
            LogManager.instance.AddLog("tension params is only for Cardinal spline, not for " + splineType.ToString() + " spline", "PathFinderToric::FindBestCurve", tension, splineType);
        }
#endif

        tension = Mathf.Clamp01(tension);
        Path path = new AStartToric(map, allowDiagonal).CalculateBestPath(start, end);

        if (path.path.Length <= 1)
        {
            return null;
        }

        List<List<MapPoint>> subPath = new List<List<MapPoint>>();
        int index = 0;
        subPath.Add(new List<MapPoint>() { path.path[0] });

        for (int i = 1; i < path.path.Length; i++)
        {
            //check for toric intersection
            if (Mathf.Max(Mathf.Abs(path.path[i].X - path.path[i - 1].X), Mathf.Abs(path.path[i].Y - path.path[i - 1].Y)) > 1)
            {
                index++;
                subPath.Add(new List<MapPoint>());
            }
            subPath[index].Add(path.path[i]);
        }

        switch (smoothnessMode)
        {
            case SmoothnessMode.None:
                break;
            case SmoothnessMode.ExtraSmoothness:
                HandleSmoothnessMode(ref subPath);
                break;
            case SmoothnessMode.Smoothnesspp:
                HandleSmoothnessModepp(ref subPath);
                break;
            default:
                break;
        }

        #region Smooth

        void HandleSmoothnessMode(ref List<List<MapPoint>> subPath)
        {
            MapPoint next, cur, prev;
            for (int i = 0; i < subPath.Count; i++)
            {
                for (int j = subPath[i].Count - 2; j >= 1; j--)
                {
                    next = subPath[i][j + 1];
                    cur = subPath[i][j];
                    prev = subPath[i][j - 1];

                    if ((next.X == cur.X && cur.X == prev.X) || (next.Y == cur.Y && cur.Y == prev.Y))
                    {
                        subPath[i].RemoveAt(j);
                    }
                }
            }
        }

        void HandleSmoothnessModepp(ref List<List<MapPoint>> subPath)
        {
            for (int i = 0; i < subPath.Count; i++)
            {
                if (subPath[i].Count <= 2)
                    continue;

                int index = subPath[i].Count - 1;
                MapPoint mapPoint;
                while(index >= 1)
                {
                    for (int j = index - 1; j >= 0; j--)
                    {
                        mapPoint = subPath[i][j];

                        if (mapPoint.X != subPath[i][index].X && mapPoint.Y != subPath[i][index].Y)
                        {
                            if(index - j <= 1)
                            {
                                index--;
                                break;
                            }

                            for (int k = index - 1; k > j; k--)
                            {
                                subPath[i].RemoveAt(k);
                            }

                            index = j;
                            break;
                        }
                    }
                    index--;
                }
            }
        }

        #endregion

        Vector2[] CreatePoints(List<MapPoint> mapPoints, Func<MapPoint, Vector2> convertMapPointToWorldPosition, bool prepolate, MapPoint previousMapPoint, bool extrapolate, MapPoint nextMapPoint)
        {
            Vector2[] points = new Vector2[(prepolate ? 1 : 0) + (extrapolate ? 1 : 0) + mapPoints.Count];

            if(prepolate)
            {
                Vector2 previousPoint = convertMapPointToWorldPosition(previousMapPoint);
                Vector2 firstPoint = convertMapPointToWorldPosition(mapPoints[0]);
                if (PhysicsToric.GetToricIntersection(previousPoint, firstPoint, out Vector2 inter))
                {
                    if(firstPoint.SqrDistance(inter) > previousPoint.SqrDistance(inter))
                        inter = PhysicsToric.GetComplementaryPoint(inter);
                    float deltaX = firstPoint.x - inter.x;
                    float deltaY = firstPoint.y - inter.y;
                    inter.x += 0.001f * deltaX.Sign();
                    inter.y += 0.001f * deltaY.Sign();
                    points[0] = PhysicsToric.GetPointInsideBounds(inter);
                }
                else
                {
#if UNITY_EDITOR
                    Debug.Log("Debug pls");
#endif
                    LogManager.instance.AddLog("PhysicsToric.GetToricIntersection(previousPoint, firstPoint, out Vector2 inter) must return true in the if(prepolate) block", previousPoint, firstPoint, "PathFindingToric::FindBestCurve::CreatePoints");
                    prepolate = false;
                    points = new Vector2[(extrapolate ? 1 : 0) + mapPoints.Count];
                }
            }

            int beg = prepolate ? 1 : 0;
            int end = extrapolate ? points.Length - 1 : points.Length;
            int offset = prepolate ? -1 : 0;
            for (int i = beg; i < end; i++)
            {
                MapPoint mapPoint = mapPoints[i + offset];
                points[i] = convertMapPointToWorldPosition(mapPoint);
            }

            if (extrapolate)
            {
                Vector2 nextPoint = convertMapPointToWorldPosition(nextMapPoint);
                Vector2 lastPoint = points[points.Length - 2];
                if (PhysicsToric.GetToricIntersection(lastPoint, nextPoint, out Vector2 inter))
                {
                    if (lastPoint.SqrDistance(inter) > nextPoint.SqrDistance(inter))
                        inter = PhysicsToric.GetComplementaryPoint(inter);
                    float deltaX = lastPoint.x - inter.x;
                    float deltaY = lastPoint.y - inter.y;
                    inter.x += 0.001f * deltaX.Sign();
                    inter.y += 0.001f * deltaY.Sign();
                    points[points.Length - 1] = PhysicsToric.GetPointInsideBounds(inter);
                }
                else
                {
#if UNITY_EDITOR
                    Debug.Log("Debug pls");
#endif
                    LogManager.instance.AddLog("PhysicsToric.GetToricIntersection(previousPoint, firstPoint, out Vector2 inter) must return true in PathFinderToric.FindBestCurve.CreatePoints in the if(extrapolate) block", nextPoint, lastPoint, "PathFindingToric::FindBestCurve::CreatePoints");
                    extrapolate = false;
                    Vector2[] tmp = new Vector2[points.Length - 1];
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        tmp[i] = points[i];
                    }
                    points = tmp;
                }
            }

            return points;
        }

        Spline CreateSpline(Vector2[] points, SplineType splineType)
        {
            switch (splineType)
            {
                case SplineType.Bezier:
                    return new HermiteSpline(points);
                case SplineType.Hermite:
                    return new HermiteSpline(points);
                case SplineType.Catmulrom:
                    return new CatmulRomSpline(points);
                case SplineType.Cardinal:
                    return new CardinalSpline(points, tension);
                case SplineType.BSline:
                    return new BSpline(points);
                default:
                    return new CatmulRomSpline(points);
            }
        }

        Spline[] splines = new Spline[subPath.Count];

        if(splines.Length <= 1)
        {
            splines[0] = CreateSpline(CreatePoints(subPath[0], convertMapPointToWorldPosition, false, null, false, null), splineType);
        }
        else if (splines.Length == 2)
        {
            splines[0] = CreateSpline(CreatePoints(subPath[0], convertMapPointToWorldPosition, false, null, true, subPath[1][0]), splineType);
            splines[1] = CreateSpline(CreatePoints(subPath[1], convertMapPointToWorldPosition, true, subPath[0].Last(), false, null), splineType);
        }
        else
        {
            splines[0] = CreateSpline(CreatePoints(subPath[0], convertMapPointToWorldPosition, false, null, true, subPath[1][0]), splineType);
            for (int i = 1; i < splines.Length - 1; i++)
            {
                splines[i] = CreateSpline(CreatePoints(subPath[i], convertMapPointToWorldPosition, true, subPath[i - 1].Last(), true, subPath[i + 1][0]), splineType);
            }
            splines[splines.Length - 1] = CreateSpline(CreatePoints(subPath[splines.Length - 1], convertMapPointToWorldPosition, true, subPath[splines.Length - 2].Last(), false, null), splineType);
        }

        return new SplinePath(path.totalCost, splines);
    }

    public class SplinePath
    {
        private Spline[] path;
        private float[] joints;
        private float[] lengthCumSum;

        public float totalCost;
        public float length { get; private set; }

        internal SplinePath(float cost, Spline[] path)
        {
            this.totalCost = cost;
            this.path = path;

            length = 0f;
            foreach (Spline s in path)
            {
                length += s.length;
            }

            if(path.Length > 1)
            {
                joints = new float[path.Length - 1];
                joints[0] = path[0].length / length;
                for (int i = 1; i < joints.Length; i++)
                {
                    joints[i] = joints[i - 1] + (path[i].length / length);
                }

                lengthCumSum = new float[path.Length];
                lengthCumSum[0] = 0f;
                for (int i = 1; i < lengthCumSum.Length; i++)
                {
                    lengthCumSum[i] = lengthCumSum[i - 1] + path[i - 1].length;
                }
            }
            else
            {
                joints = Array.Empty<float>();
                lengthCumSum = Array.Empty<float>();
            }
        }

        private (int splineIndex, float newX) GetIndexFromGlobalX(float x)
        {
            if (joints.Length <= 0)
            {
                return (0, Mathf.Clamp01(x));
            }

            x = Mathf.Clamp01(x);
            int splineIndex = joints.Length;
            for (int i = 0; i < joints.Length; i++)
            {
                if (x <= joints[i])
                {
                    splineIndex = i;
                    break;
                }
            }

            float beforeLength = lengthCumSum[splineIndex];
            float newX = (x * length - beforeLength) / path[splineIndex].length;
            return (splineIndex, newX);
        }

        public Vector2 EvaluateDistance(float x)
        {
            (int splineIndex, float newX) = GetIndexFromGlobalX(x);
            return path[splineIndex].EvaluateDistance(newX);
        }

        public Vector2[] EvaluateDistance(float[] x)
        {
            Vector2[] res = new Vector2[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                res[i] = EvaluateDistance(x[i]);
            }
            return res;
        }

        public Vector2[] EvaluateDistance(int nbPoints)
        {
            float[] x = new float[nbPoints];
            float step = 1f / (nbPoints - 1);
            for (int i = 1; i < x.Length; i++)
            {
                x[i] = x[i - 1] + step;
            }
            return EvaluateDistance(x);
        }

        public Vector2 Velocity(float x)
        {
            (int splineIndex, float newX) = GetIndexFromGlobalX(x);
            return path[splineIndex].Velocity(path[splineIndex].ConvertDistanceToTime(newX));
        }
    }
}

public class AStartToric
{
    private static readonly float sqrt2 = Mathf.Sqrt(2f);

    private AStarGraph aStar;

    #region ctor

    public AStartToric(Map map)
    {
        Builder(map, false);
    }

    public AStartToric(Map map, bool allowDiagonal = false)
    {
        Builder(map, allowDiagonal);
    }

    private void Builder(Map map, bool allowDiagonal)
    {
        if(allowDiagonal)
        {
            GenerateGraph(map);
        }
        else
        {
            GenerateGraphNonDiagonal(map);
        }
    }

    #endregion

    #region GenerateMap !diag

    private void GenerateGraphNonDiagonal(Map map)
    {
        NodeToric[,] nodes = new NodeToric[map.GetLength(0), map.GetLength(1)];
        NodeToric.mapSize = new Vector2Int(map.GetLength(0), map.GetLength(1));

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                nodes[x, y] = new NodeToric(new MapPoint(x, y));
            }
        }

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                NodeToric node = nodes[x, y];

                if (map.IsWall(node.point))
                    continue;

                NodeToric up = null, down = null, right = null, left = null;
                if(x == 0)
                {
                    up = nodes[x, y + 1];
                    down = nodes[x, y - 1];
                    right = nodes[x + 1, y];
                    left = nodes[map.GetLength(0) - 1, y];
                }
                else if (x == map.GetLength(0) - 1)
                {
                    up = nodes[x, y + 1];
                    down = nodes[x, y - 1];
                    right = nodes[0, y];
                    left = nodes[x - 1, y];
                }
                else if (y == 0)
                {
                    up = nodes[x, y + 1];
                    down = nodes[x, map.GetLength(1) - 1];
                    right = nodes[x + 1, y];
                    left = nodes[x - 1, y];
                }
                else if (y == map.GetLength(1) - 1)
                {
                    up = nodes[x, 0];
                    down = nodes[x, y - 1];
                    right = nodes[x + 1, y];
                    left = nodes[x - 1, y];
                }
                else
                {
                    up = nodes[x, y + 1];
                    down = nodes[x, y - 1];
                    right = nodes[x + 1, y];
                    left = nodes[x - 1, y];
                }

                if (!map.IsWall(up.point))
                {
                    node.AddConnection(new Edge(1f, up));
                }
                if (!map.IsWall(down.point))
                {
                    node.AddConnection(new Edge(1f, down));
                }
                if (!map.IsWall(right.point))
                {
                    node.AddConnection(new Edge(1f, right));
                }
                if (!map.IsWall(left.point))
                {
                    node.AddConnection(new Edge(1f, left));
                }
            }
        }

        List<Node> res = new List<Node>();
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                res.Add(nodes[x, y]);
            }
        }

        aStar = new AStarGraph(new Graph(res));
    }

    #endregion

    #region Generate Map

    private void GenerateGraph(Map map)
    {
        NodeToric[,] nodes = new NodeToric[map.GetLength(0), map.GetLength(1)];
        NodeToric.mapSize = new Vector2Int(map.GetLength(0), map.GetLength(1));

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                nodes[x, y] = new NodeToric(new MapPoint(x, y));
            }
        }

        NodeToric up, down, right, left, upRight, upLeft, downRight, downLeft;
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                NodeToric node = nodes[x, y];

                if (map.IsWall(node.point))
                    continue;

                up = down = right = left = upRight = upLeft = downRight = downLeft = null;
                if (x == 0)
                {
                    up = nodes[x, (y + 1) % map.GetLength(1)];
                    down = nodes[x, Useful.ClampModulo(0, map.GetLength(1), y - 1)];
                    right = nodes[x + 1, y];
                    left = nodes[map.GetLength(0) - 1, y];

                    if(y == 0)
                    {
                        upRight = nodes[x + 1, y + 1];
                        upLeft = nodes[map.GetLength(0) - 1, y + 1];
                        downRight = nodes[x + 1, map.GetLength(1) - 1];
                        downLeft = nodes[map.GetLength(0) - 1, map.GetLength(1) - 1];
                    }
                    else if (y == map.GetLength(1) - 1)
                    {
                        upRight = nodes[x + 1, 0];
                        upLeft = nodes[map.GetLength(0) - 1, 0];
                        downRight = nodes[x + 1, y - 1];
                        downLeft = nodes[map.GetLength(0) - 1, y - 1];
                    }
                    else
                    {
                        upRight = nodes[x + 1, y + 1];
                        upLeft = nodes[map.GetLength(0) - 1, y + 1];
                        downRight = nodes[x + 1, y - 1];
                        downLeft = nodes[map.GetLength(0) - 1, y - 1];
                    }
                }
                else if (x == map.GetLength(0) - 1)
                {
                    up = nodes[x, (y + 1) % map.GetLength(1)];
                    down = nodes[x, Useful.ClampModulo(0, map.GetLength(1), y - 1)];
                    right = nodes[0, y];
                    left = nodes[x - 1, y];


                    if (y == 0)
                    {
                        upRight = nodes[0, y + 1];
                        upLeft = nodes[x - 1, y + 1];
                        downRight = nodes[0, map.GetLength(1) - 1];
                        downLeft = nodes[x - 1, map.GetLength(1) - 1];
                    }
                    else if (y == map.GetLength(1) - 1)
                    {
                        upRight = nodes[0, 0];
                        upLeft = nodes[x - 1, 0];
                        downRight = nodes[0, y - 1];
                        downLeft = nodes[x - 1, y - 1];
                    }
                    else
                    {
                        upRight = nodes[0, y + 1];
                        upLeft = nodes[x - 1, y + 1];
                        downRight = nodes[0, y - 1];
                        downLeft = nodes[x - 1, y - 1];
                    }
                }
                else if (y == 0)
                {
                    int xP1 = (x + 1) % map.GetLength(0);
                    int xM1 = Useful.ClampModulo(0, map.GetLength(0), x - 1);
                    up = nodes[x, y + 1];
                    down = nodes[x, map.GetLength(1) - 1];
                    right = nodes[xP1, y];
                    left = nodes[xM1, y];
                    upRight = nodes[xP1, y + 1];
                    upLeft = nodes[xM1, y + 1];
                    downRight = nodes[xP1, map.GetLength(1) - 1];
                    downLeft = nodes[xM1, map.GetLength(1) - 1];
                }
                else if (y == map.GetLength(1) - 1)
                {
                    int xP1 = (x + 1) % map.GetLength(0);
                    int xM1 = Useful.ClampModulo(0, map.GetLength(0), x - 1);
                    up = nodes[x, 0];
                    down = nodes[x, y - 1];
                    right = nodes[xP1, y];
                    left = nodes[xM1, y];
                    upRight = nodes[xP1, 0];
                    upLeft = nodes[xM1, 0];
                    downRight = nodes[xP1, y - 1];
                    downLeft = nodes[xM1, y - 1];
                }
                else
                {
                    up = nodes[x, y + 1];
                    down = nodes[x, y - 1];
                    right = nodes[x + 1, y];
                    left = nodes[x - 1, y];
                    upRight = nodes[x + 1, y + 1];
                    upLeft = nodes[x - 1, y + 1];
                    downRight = nodes[x + 1, y - 1];
                    downLeft = nodes[x - 1, y - 1];
                }

                if (!map.IsWall(up.point))
                {
                    node.AddConnection(new Edge(1f, up));
                }
                if (!map.IsWall(down.point))
                {
                    node.AddConnection(new Edge(1f, down));
                }
                if (!map.IsWall(right.point))
                {
                    node.AddConnection(new Edge(1f, right));
                }
                if (!map.IsWall(left.point))
                {
                    node.AddConnection(new Edge(1f, left));
                }

                if (!map.IsWall(upRight.point) && !map.IsWall(up.point) && !map.IsWall(right.point))
                {
                    node.AddConnection(new Edge(sqrt2, upRight));
                }
                if (!map.IsWall(upLeft.point) && !map.IsWall(up.point) && !map.IsWall(left.point))
                {
                    node.AddConnection(new Edge(sqrt2, upLeft));
                }
                if (!map.IsWall(downRight.point) && !map.IsWall(down.point) && !map.IsWall(right.point))
                {
                    node.AddConnection(new Edge(sqrt2, downRight));
                }
                if (!map.IsWall(downLeft.point) && !map.IsWall(down.point) && !map.IsWall(left.point))
                {
                    node.AddConnection(new Edge(sqrt2, downLeft));
                }
            }
        }

        List<Node> res = new List<Node>();
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                res.Add(nodes[x, y]);
            }
        }

        aStar = new AStarGraph(new Graph(res));
    }

    #endregion

    #region Calculate Best Path

    public Path CalculateBestPath(MapPoint start, MapPoint end)
    {
        if(start == end)
        {
            return new Path(0f, new MapPoint[1] { end });
        }

        Node s = null, e = null;

        foreach (Node node in aStar.graph.nodes)
        {
            if(s == null && node.point == start)
            {
                s = node;
                continue;
            }
            if (e == null && node.point == end)
            {
                e = node;
                continue;
            }

            if (s != null && e != null)
                break;
        }


        GraphPath path = aStar.CalculateBestPath(s, e);

        MapPoint[] res = new MapPoint[path.path.Length];
        for (int i = 0; i < path.path.Length; i++)
        {
            res[i] = path.path[i].point;
        }

        return new Path(path.totalCost, res);
    }

    #endregion

    #region Private struct

    private class NodeToric : Node
    {
        public static Vector2Int mapSize;

        public NodeToric(MapPoint point) : base(point)
        {

        }

        public override int StraightLineDistanceTo(Node end)
        {
            int x = Math.Abs(end.point.X - point.X);
            int y = Math.Abs(end.point.Y - point.Y);
            return Math.Min(x, mapSize.x - x) + Math.Min(y, mapSize.y - y);
        }
    }

    #endregion
}
