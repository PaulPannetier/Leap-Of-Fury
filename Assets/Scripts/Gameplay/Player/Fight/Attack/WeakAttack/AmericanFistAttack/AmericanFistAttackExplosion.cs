using UnityEngine;

public class AmericanFistAttackExplosion : Explosion
{
    private AmericanFistAttack originalAttack;

    [SerializeField] private Color colorActivate;
    [SerializeField] private Color colorDesactivate;

    public override void Launch()
    {
        base.Launch();
    }

    public override void Launch(ExplosionData explosionData)
    {
        base.Launch(explosionData);
    }

    public void Launch(AmericanFistAttack originalAttack)
    {
        this.originalAttack = originalAttack;
        Launch();
    }

    protected override void Update()
    {
        base.Update();

        if (PauseManager.instance.isPauseEnable)
            return;

        spriteRenderer.color = enableBehaviour ? colorActivate : colorDesactivate * originalAttack.originalCloneAttack.cloneTransparency;
    }
}
