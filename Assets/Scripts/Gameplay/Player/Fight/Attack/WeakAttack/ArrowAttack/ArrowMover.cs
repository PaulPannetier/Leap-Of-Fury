using UnityEngine;

public class ArrowMover : Mover
{
    private Rigidbody2D rb;
    private ToricObject toricObject;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        toricObject = GetComponent<ToricObject>();
    }

    public override Vector2 Velocity() => toricObject.isAClone ? toricObject.original.GetComponent<ArrowMover>().Velocity() : rb.linearVelocity;
}
