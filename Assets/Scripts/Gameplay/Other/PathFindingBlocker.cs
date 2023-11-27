using System.Collections.Generic;
using UnityEngine;

namespace PathFinding
{
    public abstract class PathFindingBlocker : MonoBehaviour
    {
        private static List<PathFindingBlocker> blockers = new List<PathFindingBlocker>();

        public static List<PathFindingBlocker> GetPathFindingBlockers() => blockers;

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The list of cell where the blocker block the pathFinder algorithme, note that (0,0) is the left down corner of the map</returns>
        public abstract List<MapPoint> GetBlockedCells(Map map);

        protected virtual void Awake()
        {
            blockers.Add(this);
        }

        protected static List<MapPoint> GetBlockedCellsInRectangle(Map map, in Vector2 pos, in Vector2 size)
        {
            MapPoint topLeft = LevelMapData.currentMap.GetMapPointAtPosition(map, new Vector2(pos.x - 0.5f * size.x, pos.y + 0.5f * size.y));
            MapPoint topRight = LevelMapData.currentMap.GetMapPointAtPosition(map, new Vector2(pos.x + 0.5f * size.x, pos.y + 0.5f * size.y));
            MapPoint botLeft = LevelMapData.currentMap.GetMapPointAtPosition(map, new Vector2(pos.x - 0.5f * size.x, pos.y - 0.5f * size.y));
            MapPoint botRight = LevelMapData.currentMap.GetMapPointAtPosition(map, new Vector2(pos.x + 0.5f * size.x, pos.y - 0.5f * size.y));

            int xMin = Mathf.Min(topLeft.X, botLeft.X);
            int xMax = Mathf.Max(topRight.X, botRight.X);
            int yMin = Mathf.Min(botLeft.Y, botRight.Y);
            int yMax = Mathf.Max(topLeft.Y, topRight.Y);

            List<MapPoint> res =  new List<MapPoint>();
            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    res.Add(new MapPoint(x, y));
                }
            }
            return res;
        }

        protected static List<MapPoint> GetBlockedCellsInCircle(Map map, in Vector2 pos, float radius)
        {
            MapPoint top = LevelMapData.currentMap.GetMapPointAtPosition(map, new Vector2(pos.x, pos.y + radius ));
            MapPoint down = LevelMapData.currentMap.GetMapPointAtPosition(map, new Vector2(pos.x, pos.y - radius));
            MapPoint right = LevelMapData.currentMap.GetMapPointAtPosition(map, new Vector2(pos.x + radius , pos.y));
            MapPoint left = LevelMapData.currentMap.GetMapPointAtPosition(map, new Vector2(pos.x - radius, pos.y));

            List<MapPoint> res = new List<MapPoint>();
            for (int x = left.X; x <= right.X; x++)
            {
                for (int y = down.Y; y <= top.Y; y++)
                {
                    res.Add(new MapPoint(x, y));
                }
            }
            return res;
        }

        protected virtual void OnDestroy()
        {
            blockers.Remove(this);
        }
    }
}

