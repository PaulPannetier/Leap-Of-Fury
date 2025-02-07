#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using Collision2D;
using PathFinding;

public class PathFindingMapCreatorTest : MonoBehaviour
{
    private List<GameObject> tiles;

    [SerializeField] private GameObject squarePrefabs;
    [SerializeField] private bool generateMap;
    [SerializeField] private bool removeMap;
    [SerializeField] private bool testMapPointConvertion;

    private void GenerateTiles()
    {
        if(tiles == null)
            tiles = new List<GameObject>();

        RemoveTiles();
        Map map = LevelMapData.currentMap.GetPathfindingMap();

        MapPoint mapPoint;
        Vector2 pos;
        Vector3 scale = LevelMapData.currentMap.pathfindingCellsSize;
        int maxCost = -1;

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                maxCost = Mathf.Max(maxCost, map.GetCost(new MapPoint(x, y)));
            }
        }

         for (int x = 0; x < map.GetLength(0); x++)
         {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                mapPoint = new MapPoint(x, y);
                pos = LevelMapData.currentMap.GetPositionOfMapPoint(map, mapPoint);

                GameObject square = Instantiate(squarePrefabs, pos, Quaternion.identity, transform);
                Color color = new Color(1f, 0f, 0f, 0.75f);
                float cost = map.GetCost(mapPoint);
                if (cost >= 0)
                {
                    color = new Color(0f, 1f, 0f, (cost / (float)maxCost));
                }
                square.GetComponent<SpriteRenderer>().color = color;
                square.transform.localScale = scale;
                tiles.Add(square);
            }
         }
    }

    private void RemoveTiles()
    {
        if(tiles == null)
            return;

        foreach(GameObject tile in tiles)
        {
            Destroy(tile);
        }

        tiles.Clear();
    }

    private void OnValidate()
    {
        if(generateMap)
        {
            generateMap = false;
            GenerateTiles();
        }

        if(removeMap)
        {
            removeMap = false;
            RemoveTiles();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        if(testMapPointConvertion)
        {
            Map map = LevelMapData.currentMap.GetPathfindingMap();
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(InputManager.mousePosition);
            MapPoint currentMapPoint = LevelMapData.currentMap.GetMapPointAtPosition(map, mousePos);
            Vector2 mapPointPosition = LevelMapData.currentMap.GetPositionOfMapPoint(map, currentMapPoint);

            Circle.GizmosDraw(mousePos, 0.3f, Color.green);
            Circle.GizmosDraw(mapPointPosition, 0.2f, Color.red);
        }
    }
}

#endif
