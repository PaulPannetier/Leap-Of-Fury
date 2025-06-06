using Collision2D;
using System;

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
        jumper,
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
    public bool grabableRight = true, grabableLeft = true;
    [Range(0f, 1f), Tooltip("Le coeff de friction quand le sol se d�place")] public float frictionCoefficient = 1f;
    public bool isStatic = true;
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

    protected virtual void OnDrawGizmosSelected()
    {
        if (PrefabStageUtility.GetCurrentPrefabStage() == null)
        {
            Hitbox.GizmosDraw(Vector2.zero, LevelMapData.currentMap.mapSize * LevelMapData.currentMap.cellSize);
        }
    }

    protected virtual void OnValidate()
    {
        this.transform = base.transform;
        if (groundType == GroundType.convoyerBelt)
        {
            disableAntiKnockHead = true;
            grabableLeft = grabableRight = false;
        }

        if (groundType == GroundType.ice && !(this is IceColliderData))
        {
            print("GroundType.ice is compatible only with IceColliderData. Replace this component by an IceColliderData.");
        }
    }

#endif

    #endregion
}
