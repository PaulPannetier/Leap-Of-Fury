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

    private Vector2 oldPosition;
    private new Transform transform;

    public Vector2 velocity { get; private set; }

    public GroundType groundType;
    public bool disableAntiKnockHead = false;
    public bool grabable = true;
    [Range(0f, 1f), Tooltip("Le coeff de friction quand le sol se déplace")] public float frictionCoefficient = 0f;
    public bool isGripping => frictionCoefficient > 1e-6f;

    private void Awake()
    {
        this.transform = base.transform;
        oldPosition = transform.position;
    }

    private void Start()
    {
        EventManager.instance.callbackPreUpdate += PreUpdate;
    }

    private void PreUpdate()
    {
        velocity = new Vector2((transform.position.x - oldPosition.x) / Time.deltaTime, (transform.position.y - oldPosition.y) / Time.deltaTime);
        oldPosition = transform.position;
    }

    private void OnDestroy()
    {
        EventManager.instance.callbackPreUpdate -= PreUpdate;
    }

    #region Gizmos/OnValidate

#if UNITY_EDITOR

    private void OnValidate()
    {
        this.transform = base.transform;
        if (groundType == GroundType.convoyerBelt)
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

    #endregion
}
