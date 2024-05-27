using System;

[Serializable]
public class DisableDashEffectParams : EffectParams
{
    public float duration;

    public DisableDashEffectParams() : base()
    {

    }

    public DisableDashEffectParams(float duration) : base()
    {
        this.duration = duration;
    }

    public override void OnValidate()
    {
        duration = MathF.Max(0f, duration);
    }
}
