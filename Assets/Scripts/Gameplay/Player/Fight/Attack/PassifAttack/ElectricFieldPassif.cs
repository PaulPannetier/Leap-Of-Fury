using Collision2D;
using System.Collections.Generic;
using UnityEngine;

public class ElectricFieldPassif : PassifAttack
{
    public const bool electricFieldsAffeectAllPlayerWithThisAttack = false;

    public static List<ElectricField> electricFields {  get; private set; }

    private CharacterController characterController;

    [SerializeField] private float fieldRadius;
    [SerializeField] private float maxFieldForce;
    [SerializeField] private AnimationCurve fieldForceOverDistance;

    protected override void Awake()
    {
        base.Awake();
        characterController = GetComponent<CharacterController>();
        characterController.enableMagneticField = true;
        electricFields = new List<ElectricField>();
    }

    public void OnElectricBallCreate(ElectricBall electricBall)
    {
        int playerId = electricFieldsAffeectAllPlayerWithThisAttack ? -1 : (int)playerCommon.id;
        electricFields.Add(new ElectricField(electricBall.transform.position, fieldRadius, maxFieldForce, fieldForceOverDistance, electricBall.GetHashCode(), playerId));
    }

    public void OnElectricBallDestroy(ElectricBall electricBall)
    {
        electricFields.Remove((ElectricField e) => e.id == electricBall.GetHashCode());
    }

    protected override void Update()
    {
        base.Update();
        characterController.enableMagneticField = enableBehaviour;
    }

    #region Gizmos/OnValidate

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        fieldRadius = Mathf.Max(0f, fieldRadius);
    }

    private void OnDrawGizmosSelected()
    {
        Circle.GizmosDraw(transform.position, fieldRadius, Color.red);
    }

#endif

    #endregion

    public class ElectricField
    {
        public Vector2 center;
        public float fieldRadius, maxFieldForce;
        public AnimationCurve fieldForceOverDistance;
        public int id;
        public int playerId;

        public ElectricField(in Vector2 center, float fieldRadius, float maxFieldForce, AnimationCurve fieldForceOverDistance, int id, int playerId)
        {
            this.center = center;
            this.fieldRadius = fieldRadius;
            this.maxFieldForce = maxFieldForce;
            this.fieldForceOverDistance = fieldForceOverDistance;
            this.id = id;
            this.playerId = playerId;
        }
    }
}
