using UnityEngine;

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

#if UNITY_EDITOR

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        circleCollider = GetComponent<CircleCollider2D>();
        Gizmos.color = Color.green;
        Circle.GizmosDraw((Vector2)transform.position + circleCollider.offset, circleCollider.radius * Mathf.Max(collisionDetectionScale.x, collisionDetectionScale.y));
    }

#endif
}
