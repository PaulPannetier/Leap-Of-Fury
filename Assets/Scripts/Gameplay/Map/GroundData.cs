using UnityEngine;
using Collision2D;

public class GroundData : MonoBehaviour
{
    public static GroundData instance;

    public float iceSpeedLerpFactor = 0.5f;
    [Range(0f, 1f)] public float iceFrictionSpeedFactor = 0.985f;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Hitbox.GizmosDraw(Vector2.zero, LevelMapData.currentMap.mapSize * LevelMapData.currentMap.cellSize);
    }

#endif
}
