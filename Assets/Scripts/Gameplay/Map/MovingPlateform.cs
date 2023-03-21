using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class MovingPlateform : MonoBehaviour
{
    private BoxCollider2D hitbox;
    private Rigidbody2D rb;

    public bool enableBehaviour = true;

    [SerializeField] private float speedLerp;

    public Vector2 targetVelocity = Vector2.up;

    private void Awake()
    {
        hitbox = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!enableBehaviour)
            return;

        rb.velocity = Vector2.MoveTowards(rb.velocity, targetVelocity, speedLerp * Time.fixedDeltaTime);
    }
}
