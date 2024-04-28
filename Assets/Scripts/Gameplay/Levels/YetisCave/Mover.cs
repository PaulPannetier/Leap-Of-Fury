using UnityEngine;

public abstract class Mover : MonoBehaviour
{
    [SerializeField] public float moverForceCoeff = 1f;

    public abstract Vector2 Velocity();

#if UNITY_EDITOR

    private void OnValidate()
    {
        moverForceCoeff = Mathf.Max(0f, moverForceCoeff);
    }

#endif
}
