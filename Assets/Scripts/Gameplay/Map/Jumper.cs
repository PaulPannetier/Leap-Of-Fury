using PathFinding;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Jumper : PathFindingBlocker
{
    private BoxCollider2D hitbox;

    [Range(0f, 360f)] public float angleDir;
    public float impulseSpeed;

    protected override void Awake()
    {
        base.Awake();
        hitbox = GetComponent<BoxCollider2D>();
    }

    public override List<MapPoint> GetBlockedCells(PathFindingMap map)
    {
        return GetBlockedCellsInRectangle(map, transform.position, hitbox.size - LevelMapData.currentMap.cellSize * 0.1f);
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        impulseSpeed = Mathf.Max(0f, impulseSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Useful.GizmoDrawVector((Vector2)transform.position, new Vector2(Mathf.Cos(angleDir * Mathf.Deg2Rad), Mathf.Sin(angleDir * Mathf.Deg2Rad)));
    }

#endif
}
