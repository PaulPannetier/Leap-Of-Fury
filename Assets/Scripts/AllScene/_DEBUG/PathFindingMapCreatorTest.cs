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

    [SerializeField, Range(1, 5)] private int accuracy;

    private void GenerateTiles()
    {
        if(tiles == null)
            tiles = new List<GameObject>();

        RemoveTiles();
        Map map = LevelMapData.currentMap.GetPathfindingMap(accuracy);

        MapPoint mapPoint;
        Vector2 pos;
        Vector3 scale = LevelMapData.currentMap.cellSize / accuracy;

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                mapPoint = new MapPoint(x, y);
                pos = LevelMapData.currentMap.GetPositionOfMapPoint(map, mapPoint);

                GameObject square = Instantiate(squarePrefabs, pos, Quaternion.identity, transform);
                square.GetComponent<SpriteRenderer>().color = 0.7f * (map.IsWall(mapPoint) ? Color.red : Color.green);
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

    public Vector2 size;
    bool HPressed = false;

    private void Update()
    {
        HPressed = HPressed || InputManager.GetKeyDown(InputKey.H);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 mousePos = PhysicsToric.GetPointInsideBounds(Camera.main.ScreenToWorldPoint(InputManager.mousePosition));
        Map map = new Map(new int[0, 0], accuracy);

        if(HPressed)
        {
            HPressed = false;
        }

        List<MapPoint> points = PathFindingBlocker.GetBlockedCellsInRectangle(map, mousePos, size);
        foreach (MapPoint p in points)
        {
            Hitbox.GizmosDraw(LevelMapData.currentMap.GetPositionOfMapPoint(map, p), LevelMapData.currentMap.cellSize / accuracy, Color.red, true);
        }

        Hitbox.GizmosDraw(mousePos, size, Color.green, true);
    }
}

#endif
