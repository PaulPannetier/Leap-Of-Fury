using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] public float moverForceCoeff = 1f;

    private void OnValidate()
    {
        moverForceCoeff = Mathf.Max(0f, moverForceCoeff);
    }
}
