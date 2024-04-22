using Collision2D;
using UnityEngine;

public class MapColliderData : MonoBehaviour
{
    public enum GroundType
    {
        normal,
        ice,
        trampoline,
        convoyerBelt,
    }

    public Vector2 velocity;

    public GroundType groundType;
    public bool disableAntiKnockHead = false;
    public bool grabable = true;
    [Range(0f, 1f), Tooltip("Le coeff de friction quand le sol se déplace")] public float frictionCoefficient = 0f;
    public bool isGripping => frictionCoefficient > 1e-6f;

#if UNITY_EDITOR

    private void OnValidate()
    {
        if(groundType == GroundType.convoyerBelt)
        {
            disableAntiKnockHead = true;
            grabable = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Hitbox.GizmosDraw(Vector2.zero, LevelMapData.currentMap.mapSize * LevelMapData.currentMap.cellSize);
    }

#endif
}
