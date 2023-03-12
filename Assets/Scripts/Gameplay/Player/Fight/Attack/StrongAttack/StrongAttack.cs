using UnityEngine;
using UnityEngine.UI;

public class StrongAttack : Attack
{
    protected SliderAttackHandler sliderAttackHandler;

    protected override void Awake()
    {
        base.Awake();
        sliderAttackHandler = GetComponentInChildren<SliderAttackHandler>();
        if(cooldown.duration <= 0f)
        {
            sliderAttackHandler.enableStrongAttack = false;
        }
    }

    protected override void Update()
    {
        base.Update();
        sliderAttackHandler.SetStrongAttackSliderValue(cooldown.percentage);
    }
}
