using System;

[Serializable]
public class StunEffectParams : EffectParams
{
    public float duration;

    public StunEffectParams() : base()
    {

    }

    public StunEffectParams(float duration) : base()
    {
        this.duration = duration;
    }

    public override void OnValidate()
    {
        duration = MathF.Max(0f, duration);
    }
}
