using System;
using System.Collections.Generic;

/*
 *  Author : Bidou (http://www.csharpfr.com/auteurdetail.aspx?ID=13319)
 *  Blog   : http://blogs.developpeur.org/bidou/
 *  Date   : January 2007
 */

namespace PathFinding
{
    /// ----------------------------------------------------------------------------------------
    /// <summary>
    /// Implements the A* Algorithm.
    /// </summary>
    /// <remarks> Read the html file in the documentation directory (AStarAlgo project) for more informations. </remarks>
    /// ----------------------------------------------------------------------------------------
    public class AStar
    {
        private Map _map = null;
        private SortedNodeList<Node> _open = new SortedNodeList<Node>();
        private NodeList<Node> _close = new NodeList<Node>();

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Create a new AStar object.
        /// </summary>
        /// <param name="map"> The map. </param>
        /// ----------------------------------------------------------------------------------------
        public AStar(Map map)
        {
            if (map == null) 
                throw new ArgumentException("map cannot be null");
            this._map = map;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Create a new AStar object.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public AStar(int[,] cost)
        {
            if (cost == null) throw new ArgumentException("map cannot be null");
            this._map = new Map(cost);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Calculate the shortest path between the start point and the end point.
        /// </summary>
        /// <remarks> The path is reversed, start point not include </remarks>
        /// <returns> The shortest path. </returns>
        /// ----------------------------------------------------------------------------------------
        public MapPoint[] CalculateBestPath(MapPoint start, MapPoint end)
        {
            this._map.StartPoint = start;
            this._map.EndPoint = end;
            return CalculateBestPath();
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Calculate the shortest path between the start point and the end point.
        /// </summary>
        /// <remarks> The path is reversed, start point not include </remarks>
        /// <returns> The shortest path. </returns>
        /// ----------------------------------------------------------------------------------------
        private MapPoint[] CalculateBestPath()
        {
            Node.Map = this._map;
            Node startNode = new Node(null, this._map.StartPoint);
            this._open.Add(startNode);

            while (this._open.Count > 0)
            {
                Node best = this._open.RemoveFirst();           // This is the best node
                if (best.MapPoint == this._map.EndPoint)        // We are finished
                {
                    List<MapPoint> sol = new List<MapPoint>();  // The solution
                    while (best.Parent != null)
                    {
                        sol.Add(best.MapPoint);
                        best = best.Parent;
                    }
                    return sol.ToArray(); // Return the solution when the parent is null (the first point)
                }
                this._close.Add(best);
                this.AddToOpen(best, best.GetPossibleNode());
            }
            // No path found
            return null;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Add a list of nodes to the open list if needed.
        /// </summary>
        /// <param name="current"> The current nodes. </param>
        /// <param name="nodes"> The nodes to add. </param>
        /// ----------------------------------------------------------------------------------------
        private void AddToOpen(Node current, IEnumerable<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                if (!this._open.Contains(node))
                {
                    if (!this._close.Contains(node)) this._open.AddDichotomic(node);
                }
                // Else really nedded ?
                else
                {
                    if (node.CostWillBe() < this._open[node].Cost) node.Parent = current;
                }
            }
        }
    }
    /// ----------------------------------------------------------------------------------------
    /// <summary>
    /// Represents a MapPoint object.
    /// </summary>
    /// ----------------------------------------------------------------------------------------
    public class MapPoint
    {
        public int _x = 0;
        public int _y = 0;

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Create a new MapPoint.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public MapPoint()
        {

        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Create a new MapPoint.
        /// </summary>
        /// <param name="x"> The x-coordinate. </param>
        /// <param name="y"> The x-coordinate. </param>
        /// ----------------------------------------------------------------------------------------
        public MapPoint(int x, int y)
        {
            this._x = x;
            this._y = y;
        }

        #region Properties

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get an invalid point.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public static MapPoint InvalidPoint
        {
            get { return new MapPoint(-1, -1); }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the x-coordinate.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public int X
        {
            get { return this._x; }
            internal set { this._x = value; }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the y-coordinate.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public int Y
        {
            get { return this._y; }
            internal set { this._y = value; }
        }

        #endregion

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Operator override ! Now : value comparison.
        /// </summary>
        /// <param name="labyPt1"> The 1st point. </param>
        /// <param name="labyPt2"> The 2nd point. </param>
        /// <returns> True if the points are equals (by value!). </returns>
        /// ----------------------------------------------------------------------------------------
        public static bool operator ==(MapPoint labyPt1, MapPoint labyPt2)
        {
            return (labyPt1.X == labyPt2.X && labyPt1.Y == labyPt2.Y);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Operator override ! Now : value comparison.
        /// </summary>
        /// <param name="point1"> The 1st point. </param>
        /// <param name="point2"> The 2nd point. </param>
        /// <returns> True if the points are equals (by value!). </returns>
        /// ----------------------------------------------------------------------------------------
        public static bool operator !=(MapPoint point1, MapPoint point2)
        {
            return !(point1 == point2);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Value comparison.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns> True if the points are equals (by value!). </returns>
        /// ----------------------------------------------------------------------------------------
        public override bool Equals(object obj)
        {
            if (!(obj is MapPoint)) return false;
            MapPoint point = (MapPoint)obj;
            return point == this;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// This is the same implementation than System.Drawing.Point.
        /// </summary>
        /// <returns></returns>
        /// ----------------------------------------------------------------------------------------
        public override int GetHashCode()
        {
            return (this._x ^ this._y);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Clone the current object.
        /// </summary>
        /// <returns> A new instance with the same content. </returns>
        /// ----------------------------------------------------------------------------------------
        public MapPoint Clone()
        {
            return new MapPoint(this._x, this._y);
        }

        public override string ToString() => "{" + this._x.ToString() + ", " + this._y.ToString() + "}";
    }
    /// ----------------------------------------------------------------------------------------
    /// <summary>
    /// Define a node.
    /// </summary>
    /// <remarks> 
    /// Remember: F = Cost + Heuristic! 
    /// Read the html file in the documentation directory (AStarAlgo project) for more informations.
    /// </remarks>
    /// ----------------------------------------------------------------------------------------
    internal class Node : INode
    {
        // Represents the map
        private static Map _map = null;

        private int _costG = 0; // From start point to here
        private Node _parent = null;
        private MapPoint _currentPoint = MapPoint.InvalidPoint;

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Create a new Node.
        /// </summary>
        /// <param name="parent"> The parent node. </param>
        /// <param name="currentPoint"> The current point. </param>
        /// ----------------------------------------------------------------------------------------
        public Node(Node parent, MapPoint currentPoint)
        {
            this._currentPoint = currentPoint;
            this.SetParent(parent);
        }

        #region Properties

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get or set the Map.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public static Map Map
        {
            get { return _map; }
            set { _map = value; }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get or set the parent.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public Node Parent
        {
            get { return this._parent; }
            set { this.SetParent(value); }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the cost.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public int Cost
        {
            get { return this._costG; }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the F distance (Cost + Heuristic).
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public int F
        {
            get { return this._costG + this.GetHeuristic(); }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the location of the node.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public MapPoint MapPoint
        {
            get { return this._currentPoint; }
        }

        #endregion

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Set the parent.
        /// </summary>
        /// <param name="parent"> The parent to set. </param>
        /// ----------------------------------------------------------------------------------------
        private void SetParent(Node parent)
        {
            this._parent = parent;
            // Refresh the cost : the cost of the parent + the cost of the current point
            if (parent != null) this._costG = this._parent.Cost + _map.GetCost(this._currentPoint);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// The cost if you move to this.
        /// </summary>
        /// <returns> The futur cost. </returns>
        /// --------- -------------------------------------------------------------------------------
        public int CostWillBe()
        {
            return (this._parent != null ? this._parent.Cost + _map.GetCost(this._currentPoint) : 0);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Calculate the heuristic. (absolute x and y displacement).
        /// </summary>
        /// <returns> The heuristic. </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetHeuristic()
        {
            return (Math.Abs(this._currentPoint.X - _map.EndPoint.X) + Math.Abs(this._currentPoint.Y - _map.EndPoint.Y));
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the possible node.
        /// </summary>
        /// <returns> A list of possible node. </returns>
        /// ----------------------------------------------------------------------------------------
        public List<Node> GetPossibleNode()
        {
            List<Node> nodes = new List<Node>();
            MapPoint mapPt = new MapPoint();

            // Top
            mapPt.X = _currentPoint.X;
            mapPt.Y = _currentPoint.Y + 1;
            if (!_map.IsWall(mapPt)) nodes.Add(new Node(this, mapPt.Clone()));

            // Right
            mapPt.X = _currentPoint.X + 1;
            mapPt.Y = _currentPoint.Y;
            if (!_map.IsWall(mapPt)) nodes.Add(new Node(this, mapPt.Clone()));

            // Left
            mapPt.X = _currentPoint.X - 1;
            mapPt.Y = _currentPoint.Y;
            if (!_map.IsWall(mapPt)) nodes.Add(new Node(this, mapPt.Clone()));

            // Bottom
            mapPt.X = _currentPoint.X;
            mapPt.Y = _currentPoint.Y - 1;
            if (!_map.IsWall(mapPt)) nodes.Add(new Node(this, mapPt.Clone()));

            return nodes;
        }
    }


    /// ----------------------------------------------------------------------------------------
    /// <summary>
    /// Represents a collection of Nodes.
    /// </summary>
    /// ----------------------------------------------------------------------------------------
    internal class NodeList<T> : List<T> where T : INode
    {
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Remove and return the first node.
        /// </summary>
        /// <returns> The first Node. </returns>
        /// ----------------------------------------------------------------------------------------
        public T RemoveFirst()
        {
            T first = this[0];
            this.RemoveAt(0);
            return first;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Chek if the collection contains a Node (the MapPoint are compared by value!).
        /// </summary>
        /// <param name="node"> The node to check. </param>
        /// <returns> True if it's contained, otherwise false. </returns>
        /// ----------------------------------------------------------------------------------------
        public new bool Contains(T node)
        {
            return this[node] != null;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get a node from the collection (the MapPoint are compared by value!).
        /// </summary>
        /// <param name="node"> The node to get. </param>
        /// <returns> The node with the same MapPoint. </returns>
        /// ----------------------------------------------------------------------------------------
        public T this[T node]
        {
            get
            {
                foreach (T n in this)
                {
                    if (n.MapPoint == node.MapPoint) return n;
                }
                return default(T);
            }
        }
    }

    /// ----------------------------------------------------------------------------------------
    /// <summary>
    /// Represents a collection of SortedNodes.
    /// </summary>
    /// ----------------------------------------------------------------------------------------
    internal class SortedNodeList<T> : NodeList<T> where T : INode
    {
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Insert the node in the collection with a dichotomic algorithm.
        /// </summary>
        /// <param name="node"> The node to add.</param>
        /// ----------------------------------------------------------------------------------------
        public void AddDichotomic(T node)
        {
            int left = 0;
            int right = this.Count - 1;
            int center = 0;

            while (left <= right)
            {
                center = (left + right) / 2;
                if (node.F < this[center].F) right = center - 1;
                else if (node.F > this[center].F) left = center + 1;
                else { left = center; break; }
            }
            this.Insert(left, node);
        }
    }
    /// ----------------------------------------------------------------------------------------
    /// <summary>
    /// Define a node.
    /// </summary>
    /// ----------------------------------------------------------------------------------------
    internal interface INode
    {
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the F distance (Cost + Heuristic).
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        int F { get; }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the location of the node.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        MapPoint MapPoint { get; }
    }
    /// ----------------------------------------------------------------------------------------
    /// <summary>
    /// Represents a map
    /// </summary>
    /// ----------------------------------------------------------------------------------------
    public class Map
    {
        private int[,] _costs = null;
        private byte[,] _map = null;
        private MapPoint _startPt = MapPoint.InvalidPoint;
        private MapPoint _endPt = MapPoint.InvalidPoint;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cost">Le cout de passage d'une case, -1 pour un mur</param>
        /// <param name="start">Point de départ</param>
        /// <param name="end">Point d'arrivé</param>
        public Map(int[,] cost, MapPoint start, MapPoint end)
        {
            this._costs = cost;
            this._startPt = start;
            this._endPt = end;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cost">Le cout de passage d'une case, -1 pour un mur</param>
        /// <param name="start">Point de départ</param>
        /// <param name="end">Point d'arrivé</param>
        public Map(int[,] cost)
        {
            this._costs = cost;
        }

        #region Properties

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the length of the map.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public int Length
        {
            get { return this._costs.GetLength(0); }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the size of the map.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public int Size
        {
            get { return this.Length * this.Length; }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the start point.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public MapPoint StartPoint
        {
            get { return this._startPt; }
            set { this._startPt = value; }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the end Point.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public MapPoint EndPoint
        {
            get { return this._endPt; }
            set { this._endPt = value; }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the byte assign to a square.
        /// </summary>
        /// ----------------------------------------------------------------------------------------
        public byte this[int x, int y]
        {
            get { return this._map[x, y]; }
        }

        #endregion

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Check if a point is valid.
        /// </summary>
        /// <param name="labyPt"> The point to check. </param>
        /// <returns> True if the point is valid, otherwise false. </returns>
        /// ----------------------------------------------------------------------------------------
        public bool IsPointValid(MapPoint mapPoint)
        {
            return (this.Length > mapPoint.X && mapPoint.X >= 0 && mapPoint.Y >= 0 && this.Length > mapPoint.Y);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Check if the current point is a wall (outside point = wall).
        /// </summary>
        /// <param name="labyPt"> The point. </param>
        /// <returns> True if it is a wall. </returns>
        /// ----------------------------------------------------------------------------------------
        public bool IsWall(MapPoint mapPoint)
        {
            return this.GetCost(mapPoint) < 0;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the cost of a Point.
        /// </summary>
        /// <param name="labyPt"> The point. </param>
        /// <returns> The cost. </returns>
        /// ----------------------------------------------------------------------------------------
        public int GetCost(MapPoint mapPoint)
        {
            if (IsPointValid(mapPoint)) return this._costs[mapPoint.X, mapPoint.Y];
            return -2;
        }
    }
}