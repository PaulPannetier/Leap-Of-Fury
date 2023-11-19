using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MapColliderData : MonoBehaviour
{
    public enum GroundType
    {
        normal,
        ice,
        trampoline,
        convoyerBelt
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public GroundType groundType;
    public bool disableAntiKnockHead = false;
    public bool grabable = true;
    [Range(0f, 1f), Tooltip("Le coeff de friction quand le sol se déplace")] public float frictionCoefficient = 0f;
    public bool isGripping => frictionCoefficient > 1e-6f;
    [HideInInspector] public Rigidbody2D rb;

    private void OnValidate()
    {
        if(groundType == GroundType.convoyerBelt)
        {
            disableAntiKnockHead = true;
            grabable = false;
        }
    }
}
