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

    private ToricObject toricObject;
    private Rigidbody2D _rb;

    public Rigidbody2D rb
    {
        get
        {
            if(toricObject == null)
                return _rb;
            return toricObject.original.GetComponent<Rigidbody2D>();
        }
    }

    public GroundType groundType;
    public bool disableAntiKnockHead = false;
    public bool grabable = true;
    [Range(0f, 1f), Tooltip("Le coeff de friction quand le sol se déplace")] public float frictionCoefficient = 0f;
    public bool isGripping => frictionCoefficient > 1e-6f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        toricObject = GetComponent<ToricObject>();
    }

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
