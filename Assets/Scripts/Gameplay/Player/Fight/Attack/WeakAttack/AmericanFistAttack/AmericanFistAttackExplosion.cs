using UnityEngine;

public class AmericanFistAttackExplosion : Explosion
{
    private AmericanFistAttack originalAttack;
    private ParticleSystem[] particleSystems;

    [SerializeField] private Color colorActivate;
    [SerializeField] private Color colorDesactivate;

    protected override void Awake()
    {
        base.Awake();
        particleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    private void SetParticleSystemColor()
    {
        Color color = enableBehaviour ? colorActivate: colorDesactivate* originalAttack.originalCloneAttack.cloneTransparency;

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            try
            {
                ParticleSystem.MainModule main = particleSystem.main;
                main.startColor = color;
            }
            catch
            {

            }
        }
    }

    public void Launch(AmericanFistAttack originalAttack)
    {
        this.originalAttack = originalAttack;
        SetParticleSystemColor();
        Launch();
    }

    protected override void Update()
    {
        base.Update();

        if (PauseManager.instance.isPauseEnable)
            return;

        SetParticleSystemColor();
    }
}
