#if UNITY_EDITOR

using Collision2D;
using UnityEngine;

public class ShowMapHitbox : MonoBehaviour
{
    LevelMapData levelMapData;

    private void Awake()
    {
        Destroy(this);
    }

    private void OnValidate()
    {
        levelMapData = GetComponentInParent<LevelMapData>();
    }

    private void OnDrawGizmos()
    {
        if(levelMapData != null)
            Hitbox.GizmosDraw(Vector2.zero, levelMapData.cellSize * levelMapData.mapSize, true);
    }
}

#endif
