using PathFinding;
using PathFinding.Graph;
using System.Collections.Generic;

public static class PathFinderToric
{
    public static Path FindBestPath(Map map, MapPoint start, MapPoint end)
    {
        return new AStartToric(map).CalculateBestPath(start, end);
    }
}

public class AStartToric
{
    private AStarGraph aStar;

    public AStartToric(Map map)
    {
        GenerateGraph(map);
    }

    private void GenerateGraph(Map map)
    {
        Node[,] nodes = new Node[map.GetLength(0), map.GetLength(1)];

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                nodes[x, y] = new Node(new MapPoint(x, y));
            }
        }

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                Node node = nodes[x, y];

                if (map.IsWall(node.point))
                    continue;

                Node up = null, down = null, right = null, left = null;
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

    public Path CalculateBestPath(MapPoint start, MapPoint end)
    {
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
}
