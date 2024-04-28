using UnityEngine;

public class CharacterMover : Mover
{
    private CharacterController charController;
    private ToricObject toricObject;

    private void Awake()
    {
        charController = GetComponent<CharacterController>();
        toricObject = GetComponent<ToricObject>();
    }

    public override Vector2 Velocity() => toricObject.isAClone ? toricObject.original.GetComponent<ArrowMover>().Velocity() : charController.velocity;
}
