using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MoveWhenMoverPassThrough : MonoBehaviour
{
    [SerializeField] private float forceMultiplier = 1f;
    [SerializeField] private Vector2 maxForce = new Vector2(800f, 800f);

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Mover mover = other.gameObject.GetComponent<Mover>();
        if(mover != null)
        {
            Rigidbody2D rbOther = other.gameObject.GetComponent<Rigidbody2D>();
            Vector2 force = rbOther.velocity * (mover.moverForceCoeff * forceMultiplier);

            force.x = Mathf.Abs(force.x) > maxForce.x ? maxForce.x * force.x.Sign() : force.x;
            force.y = Mathf.Abs(force.y) > maxForce.y ? maxForce.y * force.y.Sign() : force.y;
            rb.AddForce(force);
        }
    }

    private void OnValidate()
    {
        forceMultiplier = Mathf.Max(0f, forceMultiplier);
        maxForce.x = Mathf.Max(0f, maxForce.x);
        maxForce.y = Mathf.Max(0f, maxForce.y);
    }
}
