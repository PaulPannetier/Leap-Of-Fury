using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlateform : MonoBehaviour
{
    private Rigidbody2D rb;

    public bool enableBehaviour = true;

    [SerializeField] private float speedLerp;

    public Vector2 targetVelocity = Vector2.up;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        PauseManager.instance.callBackOnPauseDisable += Enable;
        PauseManager.instance.callBackOnPauseEnable += Disable;
    }

    private void FixedUpdate()
    {
        if (!enableBehaviour)
            return;

        rb.velocity = Vector2.MoveTowards(rb.velocity, targetVelocity, speedLerp * Time.fixedDeltaTime);
    }

    private void Disable()
    {
        enableBehaviour = false;
    }

    private void Enable()
    {
        enableBehaviour = true;
    }

    private void OnDestroy()
    {
        PauseManager.instance.callBackOnPauseEnable -= Disable;
        PauseManager.instance.callBackOnPauseDisable -= Enable;
    }
}
