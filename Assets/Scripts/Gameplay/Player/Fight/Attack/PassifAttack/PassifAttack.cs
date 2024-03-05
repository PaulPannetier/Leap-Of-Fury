using UnityEngine;

public abstract class PassifAttack : Attack
{
    [SerializeField] protected bool enableBehaviour = true;

    protected override void Update()
    {
        if (enableBehaviour)
        {
            base.Update();
        }
    }
}
