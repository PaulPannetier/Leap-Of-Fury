using UnityEngine;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;
using PathFinding;
using System.Collections.Generic;
using System.Linq;

[RequireComponent (typeof(BoxCollider2D))]
public class HitboxBumpZone : BumpsZone
{
    protected BoxCollider2D hitbox;

#if UNITY_EDITOR
    [SerializeField] private bool drawGizmos;
#endif

    [SerializeField] private bool enableUpBump = true, enableDownBump = true, enableRightBump = true, enableLeftBump = true;

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
        Collider2D[] tmp = PhysicsToric.OverlapBoxAll(transform.position, hitbox.size * collisionDetectionScale, 0f, charMask);
        if (tmp.Length <= 0)
            return tmp;

        List<Collider2D> cols = tmp.ToList();
        for (int i = cols.Count - 1; i >= 0; i--)
        {
            Useful.Side side = Useful.GetRectangleSide(transform.position, hitbox.size, cols[i].transform.position);
            if (enableUpBump && side == Useful.Side.Up)
            {
                continue;
            }
            if (enableDownBump && side == Useful.Side.Down)
            {
                continue;
            }
            if (enableRightBump && side == Useful.Side.Right)
            {
                continue;
            }
            if (enableLeftBump && side == Useful.Side.Left)
            {
                continue;
            }
            cols.RemoveAt(i);
        }
        return cols.ToArray();
    }

    public override List<MapPoint> GetBlockedCells(Map map)
    {
        return GetBlockedCellsInRectangle(map, transform.position, hitbox.size - LevelMapData.currentMap.cellSize * 0.1f);
    }

    #region Gizmos

#if UNITY_EDITOR

    protected override void OnDrawGizmosSelected()
    {
        if(!drawGizmos)
            return;

        base.OnDrawGizmosSelected();
        hitbox = GetComponent<BoxCollider2D>();
        Hitbox.GizmosDraw((Vector2)transform.position + hitbox.offset, hitbox.size * collisionDetectionScale, Color.green);
    }

#endif

    #endregion
}
