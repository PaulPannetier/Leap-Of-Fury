using UnityEngine;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;

[RequireComponent (typeof(BoxCollider2D))]
public class HitboxBumpZone : BumpsZone
{
    protected BoxCollider2D hitbox;

    protected override void Awake()
    {
        base.Awake();
        hitbox = GetComponent<BoxCollider2D>();
    }

    protected override Vector2 GetColliderNormal(Collider2D charCollider)
    {
        Hitbox h = new Hitbox((Vector2)transform.position + hitbox.offset, hitbox.size);
        h.Rotate(transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
        if (Collision2D.Collider2D.CollideHitboxLine(h, (Vector2)charCollider.transform.position + charCollider.offset, h.center, out Vector2 inter))
        {
            if(h.Normal(inter, out Vector2 normal))
            {
                return normal;
            }
            Debug.LogWarning("Debug pls too");
        }
        Debug.LogWarning("Debug pls");
        Vector2 v = (Vector2)charCollider.transform.position - (Vector2)transform.position;
        float xOffset = v.x - hitbox.size.x;
        float yOffset = v.y - hitbox.size.y;
        return Mathf.Abs(xOffset) <= Mathf.Abs(yOffset) ? new Vector2(xOffset.Sign(), 0f) : new Vector2(0f, yOffset.Sign());
    }

    protected override Collider2D[] GetTouchingChar()
    {
        return PhysicsToric.OverlapBoxAll(transform.position, hitbox.size * collisionDetectionScale, 0f, charMask);
    }

#if UNITY_EDITOR

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        hitbox = GetComponent<BoxCollider2D>();
        Gizmos.color = Color.green;
        Hitbox.GizmosDraw((Vector2)transform.position + hitbox.offset, hitbox.size * collisionDetectionScale);
    }

#endif

    protected override float GetBumpTimeOffet() => (0.5f * Mathf.Max(hitbox.size.x, hitbox.size.y)) / bumpSpeed;
}
