using System;
using System.Collections.Generic;

namespace PathFinding
{
    #region PathFinder

    public static class PathFinder
    {
        public static Path FindBestPath(PathFindingMap map, MapPoint start, MapPoint end)
        {
            return new AStar(map).CalculateBestPath(start, end);
        }

        public static GraphPath FindBestPath(PathFindingGraph graph, Node start, Node end)
        {
            return new Dijkstras(graph).CalculateBestPath(start, end);
        }
    }

    #endregion

    #region Map Pathfinding

    #region A* Algo

    internal class AStar
    {
        private PathFindingMap map = null;
        private SortedNodeList<MapNode> open = new SortedNodeList<MapNode>();
        private NodeList<MapNode> close = new NodeList<MapNode>();

        public AStar(PathFindingMap map)
        {
            if (map == null)
            {
                string errorMsg = "map cannot be null";
                LogManager.instance.AddLog(errorMsg);
                map = new PathFindingMap(new int[0, 0]);
                return;
            }
            this.map = map;
        }

        public AStar(int[,] cost)
        {
            if (cost == null)
            {
                string errorMsg = "cost cannot be null";
                LogManager.instance.AddLog(errorMsg);
                map = new PathFindingMap(new int[0, 0]);
                return;
            }
            map = new PathFindingMap(cost);
        }

        public Path CalculateBestPath(MapPoint start, MapPoint end)
        {
            if (start == end)
                return new Path(0f, new MapPoint[1] { end });

            map.startPoint = start;
            map.endPoint = end;

            MapNode.map = map;
            MapNode startNode = new MapNode(null, map.startPoint);
            open.Add(startNode);

            while (open.Count > 0)
            {
                MapNode best = open.RemoveFirst();
                if (best.currentPoint == map.endPoint)
                {
                    List<MapPoint> path = new List<MapPoint>();
                    float cost = 0f;
                    while (best.parent != null)
                    {
                        path.Add(best.currentPoint);
                        cost += map.GetCost(best.currentPoint);
                        best = best.parent;
                    }
                    return new Path(cost, path.ToArray());
                }
                close.Add(best);
                AddToOpen(best, best.GetPossibleNode());
            }

            // No path found
            return new Path(-1f, Array.Empty<MapPoint>());
        }

        private void AddToOpen(MapNode current, IEnumerable<MapNode> nodes)
        {
            foreach (MapNode node in nodes)
            {
                if (!open.Contains(node))
                {
                    if (!close.Contains(node))
                        open.AddDichotomic(node);
                }
                else
                {
                    if (node.CostWillBe() < open[node].cost)
                        node.parent = current;
                }
            }
        }
    }

    #endregion

    #region MapNode

    internal class MapNode : INode
    {
        internal static PathFindingMap map = null;

        private int costG = 0; // From start point to here
        private MapNode _parent = null;
        protected MapPoint _currentPoint;
        public MapPoint currentPoint => _currentPoint;

        public int cost => costG;
        public int F => costG + GetHeuristic();
        public MapNode parent
        {
            get => _parent;
            set { SetParent(value); }
        }

        public MapNode(MapNode parent, MapPoint currentPoint)
        {
            _currentPoint = currentPoint;
            SetParent(parent);
        }

        private void SetParent(MapNode parent)
        {
            this.parent = parent;
            // Refresh the cost : the cost of the parent + the cost of the current point
            if (parent != null)
                costG = parent.cost + map.GetCost(currentPoint);
        }

        public int CostWillBe()
        {
            return parent != null ? parent.cost + map.GetCost(currentPoint) : 0;
        }

        public int GetHeuristic()
        {
            return Math.Abs(currentPoint.x - map.endPoint.x) + Math.Abs(currentPoint.y - map.endPoint.y);
        }

        public List<MapNode> GetPossibleNode()
        {
            List<MapNode> nodes = new List<MapNode>(4);
            MapPoint mapPt = new MapPoint();

            // Top
            mapPt.x = currentPoint.x;
            mapPt.y = currentPoint.y + 1;
            if (!map.IsWall(mapPt))
                nodes.Add(new MapNode(this, mapPt.Clone()));

            // Right
            mapPt.x = currentPoint.x + 1;
            mapPt.y = currentPoint.y;
            if (!map.IsWall(mapPt))
                nodes.Add(new MapNode(this, mapPt.Clone()));

            // Left
            mapPt.x = currentPoint.x - 1;
            mapPt.y = currentPoint.y;
            if (!map.IsWall(mapPt))
                nodes.Add(new MapNode(this, mapPt.Clone()));

            // Bottom
            mapPt.x = currentPoint.x;
            mapPt.y = currentPoint.y - 1;
            if (!map.IsWall(mapPt))
                nodes.Add(new MapNode(this, mapPt.Clone()));

            return nodes;
        }
    }

    #endregion

    #region NodeList

    internal class NodeList<T> : List<T> where T : INode
    {
        public T RemoveFirst()
        {
            T first = this[0];
            this.RemoveAt(0);
            return first;
        }

        public new bool Contains(T node)
        {
            return this[node] != null;
        }

        public T this[T node]
        {
            get
            {
                foreach (T n in this)
                {
                    if (n.currentPoint == node.currentPoint)
                        return n;
                }
                return default(T);
            }
        }
    }

    #endregion

    #region SortedNodeList

    internal class SortedNodeList<T> : NodeList<T> where T : INode
    {
        public void AddDichotomic(T node)
        {
            int left = 0;
            int right = Count - 1;
            int center = 0;

            while (left <= right)
            {
                center = (left + right) / 2;
                if (node.F < this[center].F)
                    right = center - 1;
                else if (node.F > this[center].F)
                    left = center + 1;
                else
                {
                    left = center;
                    break;
                }
            }
            Insert(left, node);
        }
    }

    #endregion

    #region Path

    public class Path
    {
        public float totalCost;
        public MapPoint[] path;

        public Path(float cost, MapPoint[] path)
        {
            this.totalCost = cost;
            this.path = path;
        }
    }

    #endregion

    #region MapPoint

    public struct MapPoint : ICloneable<MapPoint>
    {
        public int x, y;

        public MapPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static MapPoint InvalidPoint = new MapPoint(-1, -1);

        public static bool operator ==(MapPoint right, MapPoint left)
        {
            return right.x == left.x && right.y == left.y;
        }

        public static bool operator !=(MapPoint right, MapPoint left) => !(right == left);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, null) && ReferenceEquals(obj, null))
                return true;

            if (ReferenceEquals(this, null) || ReferenceEquals(obj, null))
                return false;

            if (obj is MapPoint point)
                return this == point;
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public MapPoint Clone()
        {
            return new MapPoint(x, y);
        }

        public override string ToString() => "{" + x.ToString() + ", " + y.ToString() + "}";
    }

    #endregion

    #region INode

    internal interface INode
    {
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the F distance (Cost + Heuristic).
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        int F { get; }

        public MapPoint currentPoint { get; }
    }

    #endregion

    #region Map

    public class PathFindingMap
    {
        private int[,] costs = null;
        private byte[,] map = null;

        public MapPoint startPoint = MapPoint.InvalidPoint;
        public MapPoint endPoint = MapPoint.InvalidPoint;
        public int Length => costs.GetLength(0);
        public int GetLength(int dimension) => this.costs.GetLength(dimension);
        public int Size => this.Length * this.Length;

        public PathFindingMap(int[,] costs, MapPoint start, MapPoint end)
        {
            this.costs = costs;
            this.startPoint = start;
            this.endPoint = end;
        }

        public PathFindingMap(int[,] costs)
        {
            this.costs = costs;
        }

        public byte this[int x, int y]
        {
            get { return this.map[x, y]; }
        }

        public bool IsPointValid(MapPoint mapPoint)
        {
            return Length > mapPoint.x && mapPoint.x >= 0 && mapPoint.y >= 0 && Length > mapPoint.y;
        }

        public bool IsWall(MapPoint mapPoint)
        {
            return GetCost(mapPoint) < 0;
        }

        public int GetCost(MapPoint mapPoint)
        {
            if (IsPointValid(mapPoint))
                return costs[mapPoint.x, mapPoint.y];
            return -2;
        }
    }

    #endregion

    #endregion

    #region Graph Search

    #region Dijkstra's Algo

    internal class Dijkstras
    {
        private PathFindingGraph graph;

        public Dijkstras(PathFindingGraph graph)
        {
            this.graph = graph;
        }

        public GraphPath CalculateBestPath(Node start, Node end)
        {
            if (start == end)
                return new GraphPath(0f, new Node[1] { end });

            Dictionary<Node, float> distances = new Dictionary<Node, float>(graph.nodes.Length);
            Dictionary<Node, Node> previous = new Dictionary<Node, Node>(graph.nodes.Length);
            HashSet<Node> visited = new HashSet<Node>(graph.nodes.Length);
            SortedSet<PathNode> priorityQueue = new SortedSet<PathNode>(new PathNodeComparer());

            uint id = 0u;
            foreach (Node node in graph.nodes)
            {
                distances[node] = float.MaxValue;
                previous[node] = null;
                node.id = id;
                id++;
            }

            distances[start] = 0f;
            priorityQueue.Add(new PathNode(start, 0f));

            while (priorityQueue.Count > 0)
            {
                PathNode currentPathNode = priorityQueue.Min;
                priorityQueue.Remove(currentPathNode);
                Node current = currentPathNode.node;

                if (current == end)
                    break;

                if (!visited.Add(current))
                    continue;

                List<Edge> connections = current.connections;
                int connectionCount = connections.Count;
                for (int i = 0; i < connectionCount; i++)
                {
                    Edge edge = connections[i];
                    Node neighbor = edge.connectedNode;

                    if (visited.Contains(neighbor))
                        continue;

                    float newDist = distances[current] + edge.cost;
                    if (newDist < distances[neighbor])
                    {
                        priorityQueue.Remove(new PathNode(neighbor, distances[neighbor]));
                        distances[neighbor] = newDist;
                        previous[neighbor] = current;
                        priorityQueue.Add(new PathNode(neighbor, newDist));
                    }
                }
            }

            if (distances[end] >= float.MaxValue)
                return new GraphPath(-1f, Array.Empty<Node>()); // No path found

            List<Node> path = new List<Node>();
            for (Node at = end; at != null; at = previous[at])
            {
                path.Add(at);
            }
            path.Reverse();

            return new GraphPath(distances[end], path.ToArray());
        }
    }

    #endregion

    #region PathFindingGraph

    public class PathFindingGraph
    {
        public Node[] nodes;

        public PathFindingGraph(Node[] nodes)
        {
            this.nodes = nodes;
        }
    }

    #endregion

    #region Path

    public class GraphPath
    {
        public float totalCost;
        public Node[] path;

        public GraphPath(float totalCost, Node[] path)
        {
            this.totalCost = totalCost;
            this.path = path;
        }
    }

    #endregion

    #region Node

    public class Node
    {
        public List<Edge> connections { get; private set; }
        internal uint id;

        public Node()
        {
            connections = new List<Edge>();
        }

        public void AddConnection(Edge edge)
        {
            connections.Add(edge);
        }
    }

    #endregion

    #region PathNode

    internal class PathNode
    {
        public Node node;
        public float cost;

        public PathNode(Node node, float cost)
        {
            this.node = node;
            this.cost = cost;
        }
    }

    internal class PathNodeComparer : IComparer<PathNode>
    {
        public int Compare(PathNode x, PathNode y)
        {
            int compare = x.cost.CompareTo(y.cost);
            return compare == 0 ? x.node.id.CompareTo(y.node.id) : compare;
        }
    }

    #endregion

    #region Edge

    public class Edge
    {
        public float cost;
        public Node connectedNode;

        public Edge(float cost, Node connectedNode)
        {
            this.cost = cost;
            this.connectedNode = connectedNode;
        }
    }

    #endregion

    #endregion
}