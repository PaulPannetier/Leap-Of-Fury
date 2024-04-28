using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MoveWhenMoverPassThrough : MonoBehaviour
{
    private Rigidbody2D rb;

    public bool enableBehaviour = true;
    [SerializeField] private float forceMultiplier = 1f;
    [SerializeField] private Vector2 maxForce = new Vector2(800f, 800f);

    public bool moveOnExplosion = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        PauseManager.instance.callBackOnPauseDisable += Enable;
        PauseManager.instance.callBackOnPauseEnable += Disable;
        ExplosionManager.AddMovingWithExplosionRidgidbody(rb);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Mover mover = other.GetComponent<Mover>();
        if(mover != null)
        {
            Vector2 force = mover.Velocity() * (mover.moverForceCoeff * forceMultiplier);

            force.x = Mathf.Abs(force.x) > maxForce.x ? maxForce.x * force.x.Sign() : force.x;
            force.y = Mathf.Abs(force.y) > maxForce.y ? maxForce.y * force.y.Sign() : force.y;
            rb.AddForce(force);
        }
    }

    private void Enable()
    {
        enableBehaviour = true;
    }

    private IEnumerator DisableCorout()
    {
        enableBehaviour = false;
        Vector2 speed = rb.velocity;
        float angularSpeed = rb.angularVelocity;
        RigidbodyConstraints2D rbConstrain = rb.constraints;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        while (!enableBehaviour)
        {
            yield return null;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        rb.velocity = speed;
        rb.angularVelocity = angularSpeed;
        rb.constraints = rbConstrain;
    }

    private void Disable()
    {
        StartCoroutine(DisableCorout());
    }

    private void OnDestroy()
    {
        PauseManager.instance.callBackOnPauseEnable -= Disable;
        PauseManager.instance.callBackOnPauseDisable -= Enable;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        forceMultiplier = Mathf.Max(0f, forceMultiplier);
        maxForce.x = Mathf.Max(0f, maxForce.x);
        maxForce.y = Mathf.Max(0f, maxForce.y);
    }

#endif

}
