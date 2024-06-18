using UnityEngine;

public class IceColliderData : MapColliderData
{
    [Header("Ice")]
    [Range(0f, 1f)] public float iceSpeedLerpFactor;
    [Range(0f, 1f)] public float iceDecelerationSpeedLerpFactor;

    #region Gizmos/OnValidate

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        if (groundType != GroundType.ice)
        {
            print("IceColliderData is compatible only GroundType.ice. Replace this component by a regular MapColliderData.");
        }
    }

#endif

    #endregion
}
