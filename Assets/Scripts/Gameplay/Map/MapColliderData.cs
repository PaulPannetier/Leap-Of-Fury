using Collision2D;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

public class MapColliderData : MonoBehaviour
{
    public enum GroundType
    {
        normal,
        ice,
        trampoline,
        convoyerBelt,
        oneWayPlateform
    }

    private Vector2 oldPosition;
    private ToricObject toricObject;
    private new Transform transform;

    private Vector2 _velocity;
    public Vector2 velocity
    {
        get => toricObject != null ? (toricObject.isAClone ? toricObject.original.GetComponent<MapColliderData>()._velocity : _velocity) : _velocity;
        private set { _velocity = value; }
    }

    public GroundType groundType;
    public bool disableAntiKnockHead = false;
    public bool grabable = true;
    [Range(0f, 1f), Tooltip("Le coeff de friction quand le sol se déplace")] public float frictionCoefficient = 0f;
    public bool isGripping => frictionCoefficient > 1e-6f;

    private void Awake()
    {
        this.transform = base.transform;
        toricObject = GetComponent<ToricObject>();
        oldPosition = transform.position;
    }

    private void Update()
    {
        Vector2 dir = PhysicsToric.Direction(oldPosition, transform.position);
        float dist = PhysicsToric.Distance(oldPosition, transform.position);
        velocity = dir * (dist / Time.deltaTime);
        oldPosition = transform.position;
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
        if(PrefabStageUtility.GetCurrentPrefabStage() == null)
        {
            Hitbox.GizmosDraw(Vector2.zero, LevelMapData.currentMap.mapSize * LevelMapData.currentMap.cellSize);
        }
    }

#endif

    #endregion
}
