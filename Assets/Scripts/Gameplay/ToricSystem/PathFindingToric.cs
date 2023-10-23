using PathFinding;
using PathFinding.Graph;
using System.Collections.Generic;
using System;
using UnityEngine;

public static class PathFinderToric
{
    public static Path FindBestPath(Map map, MapPoint start, MapPoint end, bool allowDiagonal = false)
    {
        return new AStartToric(map, allowDiagonal).CalculateBestPath(start, end);
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
                    up = nodes[x, y + 1];
                    down = nodes[x, y - 1];
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
                    up = nodes[x, y + 1];
                    down = nodes[x, y - 1];
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
                    up = nodes[x, y + 1];
                    down = nodes[x, map.GetLength(1) - 1];
                    right = nodes[x + 1, y];
                    left = nodes[x - 1, y];
                    upRight = nodes[x + 1, y + 1];
                    upLeft = nodes[x - 1, y + 1];
                    downRight = nodes[x + 1, map.GetLength(1) - 1];
                    downLeft = nodes[x - 1, map.GetLength(1) - 1];
                }
                else if (y == map.GetLength(1) - 1)
                {
                    up = nodes[x, 0];
                    down = nodes[x, y - 1];
                    right = nodes[x + 1, y];
                    left = nodes[x - 1, y];
                    upRight = nodes[x + 1, 0];
                    upLeft = nodes[x - 1, 0];
                    downRight = nodes[x + 1, y - 1];
                    downLeft = nodes[x - 1, y - 1];
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
