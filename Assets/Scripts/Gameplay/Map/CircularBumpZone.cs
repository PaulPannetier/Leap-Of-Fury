using UnityEngine;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;
using System.Collections.Generic;
using PathFinding;

[RequireComponent (typeof(CircleCollider2D))]
public class CircularBumpZone : BumpsZone
{
    protected CircleCollider2D circleCollider;

    protected override void Awake()
    {
        base.Awake();
        circleCollider = GetComponent<CircleCollider2D>();
    }

    protected override Vector2 GetColliderNormal(Collider2D charCollider)
    {
        return (((Vector2)charCollider.transform.position + charCollider.offset) - (Vector2)transform.position).normalized;
    }

    protected override Collider2D[] GetTouchingChar()
    {
        return PhysicsToric.OverlapCircleAll(transform.position, circleCollider.radius * Mathf.Max(collisionDetectionScale.x, collisionDetectionScale.y), charMask);
    }

    protected override float GetBumpTimeOffet() => (circleCollider.radius * Mathf.Max(collisionDetectionScale.x, collisionDetectionScale.y) - circleCollider.radius) / bumpSpeed;

    public override List<MapPoint> GetBlockedCells()
    {
        Vector2 cellsSize = LevelMapData.currentMap.cellSize;
        Vector2 mapSize = LevelMapData.currentMap.mapSize;
        Vector2Int mapCellsSize = new Vector2Int((mapSize.x / cellsSize.x).Round(), (mapSize.y / cellsSize.y).Round());

        Vector2 origin = PhysicsToric.GetPointInsideBounds(transform.position) + LevelMapData.currentMap.mapSize * 0.5f;
        Vector2Int coord = new Vector2Int((int)(origin.x / LevelMapData.currentMap.cellSize.x), (int)(origin.y / LevelMapData.currentMap.cellSize.y));

        List<MapPoint> res = new List<MapPoint>();
        Vector2Int zoneCells = new Vector2Int((circleCollider.radius / cellsSize.x).Round(), (circleCollider.radius / cellsSize.y).Round());

        for (int i = 0; i < zoneCells.x; i++)
        {
            for (int j = 0; j < zoneCells.y; j++)
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

    #region Gizmos

#if UNITY_EDITOR

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        circleCollider = GetComponent<CircleCollider2D>();
        Gizmos.color = Color.green;
        Circle.GizmosDraw((Vector2)transform.position + circleCollider.offset, circleCollider.radius * Mathf.Max(collisionDetectionScale.x, collisionDetectionScale.y));
    }

#endif

    #endregion
}
