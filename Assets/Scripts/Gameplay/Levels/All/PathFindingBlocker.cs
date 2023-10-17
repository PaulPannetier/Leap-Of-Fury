using System.Collections.Generic;
using UnityEngine;

namespace PathFinding
{
    public abstract class PathFindingBlocker : MonoBehaviour
    {
        private static List<PathFindingBlocker> blockers = new List<PathFindingBlocker>();
        private static readonly float sqrt2 = Mathf.Sqrt(2f); 

        public static List<PathFindingBlocker> GetPathFindingBlockers() => blockers;

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The list of cell where the blocker block the pathFinder algorithme, note that (0,0) is the left down corner of the map</returns>
        public abstract List<MapPoint> GetBlockedCells();

        protected virtual void Awake()
        {
            blockers.Add(this);
        }

        protected static List<MapPoint> GetBlockedCellsInRectangle(in Vector2 pos, in Vector2 size)
        {
            Vector2 cellsSize = LevelMapData.currentMap.cellSize;
            Vector2 mapSize = LevelMapData.currentMap.mapSize;
            Vector2Int mapCellsSize = new Vector2Int((mapSize.x / cellsSize.x).Round(), (mapSize.y / cellsSize.y).Round());

            List<MapPoint> res = new List<MapPoint>();
            Vector2Int hitboxCells = new Vector2Int((size.x / cellsSize.x).Round(), (size.y / cellsSize.y).Round());

            int maxX = ((hitboxCells.x - 1) * 0.5f).Ceil();
            int maxY = ((hitboxCells.y - 1) * 0.5f).Ceil();

            List<Vector2Int> begCoor = new List<Vector2Int>
            {
                GetCellAtPosition(new Vector2(pos.x + 0.4f * cellsSize.x, pos.y)),
                GetCellAtPosition(new Vector2(pos.x - 0.4f * cellsSize.x, pos.y)),
                GetCellAtPosition(new Vector2(pos.x, pos.y + 0.4f * cellsSize.y)),
                GetCellAtPosition(new Vector2(pos.x, pos.y - 0.4f * cellsSize.y))
            }.Distinct();

            foreach (Vector2Int beg in begCoor)
            {
                for (int i = -maxX; i <= maxX; i++)
                {
                    for (int j = -maxY; j <= maxY; j++)
                    {
                        Vector2Int coor = new Vector2Int(beg.x + i, beg.y + j);
                        if (coor.x >= 0 && coor.x < mapCellsSize.x && coor.y >= 0 && coor.y < mapCellsSize.y)
                        {
                            MapPoint mapPoint = new MapPoint(coor.x, coor.y);
                            if (!res.Contains(mapPoint))
                                res.Add(mapPoint);
                        }
                    }
                }
            }

            return res;

            Vector2Int GetCellAtPosition(in Vector2 pos)
            {
                Vector2 origin = PhysicsToric.GetPointInsideBounds(pos) + mapSize * 0.5f;
                return new Vector2Int((int)(origin.x / cellsSize.x), (int)(origin.y / cellsSize.y));
            }
        }

        protected static List<MapPoint> GetBlockedCellsInCircle(in Vector2 pos, float radius)
        {
            return GetBlockedCellsInRectangle(pos, new Vector2(radius * sqrt2, radius * sqrt2));
        }
    }

}

