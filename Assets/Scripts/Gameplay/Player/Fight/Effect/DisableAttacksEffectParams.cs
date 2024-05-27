using System;

[Serializable]
public class DisableAttacksEffectParams : EffectParams
{
    public enum DisableAttack
    {
        None,
        WeakAttack,
        StrongAttack,
        Both
    }

    public DisableAttack disableAttack;
    public float duration;

    public DisableAttacksEffectParams() : base()
    {

    }

    public DisableAttacksEffectParams(float duration) : base()
    {
        this.duration = duration;
    }

    public override void OnValidate()
    {
        duration = MathF.Max(0f, duration);
    }
}
