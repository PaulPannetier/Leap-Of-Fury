using System.Collections.Generic;
using System.Linq;
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

        private struct Rectangle
        {
            public Vector2 center, size;

            public Rectangle(Vector2 center, Vector2 size)
            {
                this.center = center;
                this.size = size;
            }
        }

        public static List<MapPoint> GetBlockedCellsInRectangle(Map map, in Vector2 pos, in Vector2 size)
        {
            //1 Slice the hitbox into multiple to fix the toric Space
            List<Rectangle> rectangles = SliceRectangle(new Rectangle(PhysicsToric.GetPointInsideBounds(pos), size));

            List<Rectangle> SliceRectangle(Rectangle rectangle)
            {       
                List<Rectangle> res = new List<Rectangle>() { };
                Vector2 mapSize = LevelMapData.currentMap.mapSize * LevelMapData.currentMap.cellSize;
                float xMax = 0.5f * mapSize.x;
                float xMin = -xMax;
                float yMax = 0.5f * mapSize.y;
                float yMin = -yMax;
                float value;
                Rectangle currentRec;

                List<Rectangle> recToSlice = new List<Rectangle>(1) { rectangle };
                List<Rectangle> newRecToSlice = new List<Rectangle>(1);

                while (recToSlice.Count > 0)
                {
                    for (int i = recToSlice.Count - 1; i >= 0 ; i--)
                    {
                        currentRec = recToSlice[i];
                        bool isCurrentRecSlice = false;

                        //left
                        value = currentRec.center.x - (currentRec.size.x * 0.5f);
                        if (value < xMin)
                        {
                            float size2 = xMin - value;
                            float size1 = currentRec.size.x - size2;
                            Rectangle rec1 = new Rectangle(new Vector2(xMin + (size1 * 0.5f), currentRec.center.y), new Vector2(size1, currentRec.size.y));
                            Rectangle rec2 = new Rectangle(new Vector2(value + (size2 * 0.5f) + mapSize.x, currentRec.center.y), new Vector2(size2, currentRec.size.y));
                            newRecToSlice.Add(rec1);
                            newRecToSlice.Add(rec2);
                            isCurrentRecSlice = true;
                        }

                        //right
                        value = currentRec.center.x + (currentRec.size.x * 0.5f);
                        if (value > xMax)
                        {
                            float size2 = value - xMax;
                            float size1 = currentRec.size.x - size2;
                            Rectangle rec1 = new Rectangle(new Vector2(xMax - (0.5f * size1), currentRec.center.y), new Vector2(size1, currentRec.size.y));
                            Rectangle rec2 = new Rectangle(new Vector2(value - (size2 * 0.5f) - mapSize.x, currentRec.center.y), new Vector2(size2, currentRec.size.y));
                            newRecToSlice.Add(rec1);
                            newRecToSlice.Add(rec2);
                            isCurrentRecSlice = true;
                        }

                        //down
                        value = currentRec.center.y - (currentRec.size.y * 0.5f);
                        if (value < yMin)
                        {
                            float size2 = yMin - value;
                            float size1 = currentRec.size.y - size2;
                            Rectangle rec1 = new Rectangle(new Vector2(currentRec.center.x, yMin + (0.5f * size1)), new Vector2(currentRec.size.x, size1));
                            Rectangle rec2 = new Rectangle(new Vector2(currentRec.center.x, value + (size2 * 0.5f) + mapSize.y), new Vector2(currentRec.size.x, size2));
                            newRecToSlice.Add(rec1);
                            newRecToSlice.Add(rec2);
                            isCurrentRecSlice = true;
                        }

                        //up
                        value = currentRec.center.y + (currentRec.size.y * 0.5f);
                        if (value > yMax)
                        {
                            float size2 = value - yMax;
                            float size1 = currentRec.size.y - size2;
                            Rectangle rec1 = new Rectangle(new Vector2(currentRec.center.x, yMax - (size1 * 0.5f)), new Vector2(currentRec.size.x, size1));
                            Rectangle rec2 = new Rectangle(new Vector2(currentRec.center.x, value - (size2 * 0.5f) - mapSize.y), new Vector2(currentRec.size.x, size2));
                            newRecToSlice.Add(rec1);
                            newRecToSlice.Add(rec2);
                            isCurrentRecSlice = true;
                        }

                        if(!isCurrentRecSlice)
                        {
                            res.Add(currentRec);
                            recToSlice.RemoveAt(i);
                        }
                    }

                    recToSlice.Clear();
                    List<Rectangle> tmp = recToSlice;
                    recToSlice = newRecToSlice;
                    newRecToSlice = tmp;
                }

                return res;
            }

            HashSet<MapPoint> res = new HashSet<MapPoint>();

            for (int i = 0; i < rectangles.Count; i++)
            {
                List<MapPoint> mapPoints = GetBlockedCellsInRectangleInternal(map, rectangles[i]);
                for (int j = 0; j < mapPoints.Count; j++)
                {
                    res.Add(mapPoints[j]);
                }
            }

            return res.ToList();
        }

        //assume that the rectangle is INSIDE the tore map space
        private static List<MapPoint> GetBlockedCellsInRectangleInternal(Map map, Rectangle rec)
        {
            rec.size *= 0.999f;

            MapPoint topLeft = LevelMapData.currentMap.GetMapPointAtPosition(map, new Vector2(rec.center.x - 0.5f * rec.size.x, rec.center.y + 0.5f * rec.size.y));
            MapPoint topRight = LevelMapData.currentMap.GetMapPointAtPosition(map, new Vector2(rec.center.x + 0.5f * rec.size.x, rec.center.y + 0.5f * rec.size.y));
            MapPoint botLeft = LevelMapData.currentMap.GetMapPointAtPosition(map, new Vector2(rec.center.x - 0.5f * rec.size.x, rec.center.y - 0.5f * rec.size.y));
            MapPoint botRight = LevelMapData.currentMap.GetMapPointAtPosition(map, new Vector2(rec.center.x + 0.5f * rec.size.x, rec.center.y - 0.5f * rec.size.y));

            int xMin = Mathf.Min(topLeft.X, botLeft.X);
            int xMax = Mathf.Max(topRight.X, botRight.X);
            int yMin = Mathf.Min(botLeft.Y, botRight.Y);
            int yMax = Mathf.Max(topLeft.Y, topRight.Y);

            List<MapPoint> res = new List<MapPoint>();
            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    res.Add(new MapPoint(x, y));
                }
            }
            return res;
        }

        public static List<MapPoint> GetBlockedCellsInCircle(Map map, in Vector2 pos, float radius)
        {
            float cache = 2f * radius;
            List<MapPoint> res = GetBlockedCellsInRectangle(map, pos, new Vector2(cache, cache));
            float d = LevelMapData.currentMap.cellSize.x * Mathf.Sqrt(0.5f) / map.accuracy;
            cache = Mathf.Sqrt(radius) + d;

            for (int i = res.Count - 1; i >= 0; i--)
            {
                d = PhysicsToric.Distance(pos, LevelMapData.currentMap.GetPositionOfMapPoint(map, res[i]));
                if(d > cache)
                {
                    res.RemoveAt(i);
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

