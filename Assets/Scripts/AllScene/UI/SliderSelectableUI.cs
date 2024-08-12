using UnityEngine;
using UnityEngine.UI;

public class SliderSelectableUI : SelectableUI
{
    private bool isActive = false;

    [SerializeField] private Slider slider;

    [SerializeField] private InputManager.GeneralInput inputIncrease;
    [SerializeField] private InputManager.GeneralInput inputDecrease;
    [SerializeField] private InputManager.GeneralInput inputDesactive;
    [SerializeField] private float durationToFill = 1f;

    public override void OnPressed()
    {
        if(isSelected)
        {
            selectableUIGroup.enableBehaviour = false;
            isActive = true;
        }
    }

    private void Update()
    {
        if (!isActive)
            return;

        if(inputDesactive.IsPressedDown())
        {
            isActive = false;
            selectableUIGroup.enableBehaviour = true;
        }

        if(inputDecrease.IsPressed())
        {
            slider.value = Mathf.Max(0f, slider.value - (Time.deltaTime * durationToFill));
        }

        if(inputIncrease.IsPressed())
        {
            slider.value = Mathf.Min(1f, slider.value + (Time.deltaTime * durationToFill));
        }
    }

    #region OnValidate

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        durationToFill = Mathf.Max(0f, durationToFill);
    }

#endif

    #endregion
}
