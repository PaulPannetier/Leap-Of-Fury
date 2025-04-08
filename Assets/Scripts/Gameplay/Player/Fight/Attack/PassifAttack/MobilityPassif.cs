using UnityEngine;

public class MobilityPassif : PassifAttack
{
    private CharacterController charController;

    [SerializeField, Tooltip("Walk speed bonus in %age")] private float walkSpeedBonus;
    [SerializeField, Tooltip("Grab speed bonus in %age")] private float grabSpeedBonus;

    protected override void Awake()
    {
        base.Awake();
        charController = GetComponent<CharacterController>();
    }

    protected override void Start()
    {
        base.Start();

        charController.AddBonusPercent(CharacterController.BonusType.Walk, walkSpeedBonus);
        charController.AddBonusPercent(CharacterController.BonusType.Grab, grabSpeedBonus);
    }

    #region OnValidate

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        walkSpeedBonus = Mathf.Max(walkSpeedBonus, 0f);
        grabSpeedBonus = Mathf.Max(grabSpeedBonus, 0f);
    }

#endif

    #endregion
}
