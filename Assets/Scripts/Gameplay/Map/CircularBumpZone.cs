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

    public override List<MapPoint> GetBlockedCells(PathFindingMap map)
    {
        return GetBlockedCellsInCircle(map, transform.position, circleCollider.radius);
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
