using UnityEngine.UI;
using UnityEngine;

public class WeakAttack : Attack
{
    protected SliderAttackHandler sliderAttackHandler;

    protected override void Awake()
    {
        base.Awake();
        sliderAttackHandler = GetComponentInChildren<SliderAttackHandler>();
        if (cooldown.duration <= 0f)
        {
            sliderAttackHandler.enableWeakAttack = false;
        }
    }

    protected override void Update()
    {
        base.Update();
        sliderAttackHandler.SetWeakAttackSliderValue(cooldown.percentage);
    }
}