using UnityEngine;

[RequireComponent(typeof(MapColliderData))]
public class ConvoyerBelt : MonoBehaviour
{
    public bool enableBehaviour = true;
    public float maxSpeed;
    public float speedLerp;

    private void OnValidate()
    {
        speedLerp = Mathf.Max(speedLerp, 0f);
    }
}
