using UnityEngine;

public class ActivableBomb : ActivableObject
{
    [SerializeField] private float explosionRadius;
    [SerializeField] private Vector2 explosionOffset;

    protected override void OnActivated()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnDesactivated()
    {
        throw new System.NotImplementedException();
    }

    #region OnValidate

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        explosionRadius = Mathf.Max(explosionRadius, 0f);
    }

#endif

    #endregion
}
