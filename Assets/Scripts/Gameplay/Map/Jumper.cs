using PathFinding;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Jumper : PathFindingBlocker
{
    private BoxCollider2D hitbox;

    [Range(0f, 360f)] public float angleDir;
    public float force;

    protected override void Awake()
    {
        base.Awake();
        hitbox = GetComponent<BoxCollider2D>();
    }

    public override List<MapPoint> GetBlockedCells()
    {
        Vector2 cellsSize = LevelMapData.currentMap.cellSize;
        Vector2 mapSize = LevelMapData.currentMap.mapSize;
        Vector2Int mapCellsSize = new Vector2Int((mapSize.x / cellsSize.x).Round(), (mapSize.y / cellsSize.y).Round());

        Vector2 origin = PhysicsToric.GetPointInsideBounds(transform.position) + LevelMapData.currentMap.mapSize * 0.5f;
        Vector2Int coord = new Vector2Int((int)(origin.x / LevelMapData.currentMap.cellSize.x), (int)(origin.y / LevelMapData.currentMap.cellSize.y));

        List<MapPoint> res = new List<MapPoint>();
        Vector2Int hitboxCells = new Vector2Int((hitbox.size.x / cellsSize.x).Round(), (hitbox.size.y / cellsSize.y).Round());

        for (int i = 0; i < hitboxCells.x; i++)
        {
            for (int j = 0; j < hitboxCells.y; j++)
            {
                int cellX = coord.x - i;
                int cellY = coord.y - j;
                if (cellX >= 0 && cellX < mapCellsSize.x && cellY >= 0 & cellY < mapCellsSize.y)
                {
                    res.Add(new MapPoint(cellX, cellY));
                }
            }
        }

        return res;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Useful.GizmoDrawVector((Vector2)transform.position, new Vector2(Mathf.Cos(angleDir * Mathf.Deg2Rad), Mathf.Sin(angleDir * Mathf.Deg2Rad)));
    }
}
