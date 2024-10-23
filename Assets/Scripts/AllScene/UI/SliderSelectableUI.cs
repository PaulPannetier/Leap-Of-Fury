using UnityEngine;
using UnityEngine.UI;

public class SliderSelectableUI : SelectableUI
{
    private bool isActivatedThisFrame = false;
    [SerializeField] private Slider slider;

    [SerializeField] private InputManager.GeneralInput inputIncrease;
    [SerializeField] private InputManager.GeneralInput inputDecrease;
    [SerializeField] private InputManager.GeneralInput inputDesactive;
    [SerializeField] private float durationToFill = 1f;

    public float value
    {
        get => slider.value;
        set => slider.value = value;
    }

    public override bool interactable
    {
        get => base.interactable;
        set
        {
            base.interactable = value;
            slider.interactable = value;
        }
    }

    public override SelectableUIGroup selectableUIGroup
    { 
        get => base.selectableUIGroup;
        set
        {
            base.selectableUIGroup = value;
            if (value != null)
            {
                ControllerType controllerType = ControllerType.Keyboard;
                if (value.allowedController == BaseController.KeyboardAndGamepad)
                    controllerType = ControllerType.Any;
                else if (value.allowedController == BaseController.Gamepad)
                    controllerType = ControllerType.GamepadAny;

                inputIncrease.controllerType = inputDecrease.controllerType = inputDesactive.controllerType = controllerType;
            }
        }    
    }

#if UNITY_EDITOR
    public bool generateDefaultSliderColorFaders = false;
#endif

    public override void OnPressedUp()
    {

    }

    public override void OnPressed()
    {
        if (isSelected && !isDesactivatedThisFrame)
        {
            isActive = true;
            isActivatedThisFrame = true;
        }
    }

    private void Update()
    {
        isDesactivatedThisFrame = false;

        if (!isActive)
        {
            isActivatedThisFrame = false;
            return;
        }

        if (inputDesactive.IsPressedDown() && !isActivatedThisFrame)
        {
            isActive = false;
            isDesactivatedThisFrame = true;
        }

        if (inputDecrease.IsPressed())
        {
            slider.value = Mathf.Max(0f, slider.value - (Time.deltaTime / durationToFill));
        }

        if(inputIncrease.IsPressed())
        {
            slider.value = Mathf.Min(1f, slider.value + (Time.deltaTime / durationToFill));
        }

        isActivatedThisFrame = false;
    }

    #region OnValidate

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        if(slider == null)
        {
            slider = GetComponentInChildren<Slider>();
        }

        durationToFill = Mathf.Max(0f, durationToFill);

        if(generateDefaultSliderColorFaders)
        {
            generateDefaultSliderColorFaders = false;

            Image handleImage = slider.transform.GetChild(2).GetChild(0).GetComponent<Image>();
            ColorFader handleFader = new ColorFader(Color.white, new Color(0.9607843f, 0.9607843f, 0.9607843f), new Color(0.7843137f, 0.7843137f, 0.7843137f), new Color(0.9607843f, 0.9607843f, 0.9607843f), new Color(0.94117647f, 0.94117647f, 0.94117647f), null, handleImage, 0.1f);
            Image fillImage = slider.transform.GetChild(1).GetChild(0).GetComponent<Image>();
            ColorFader fillAreaFader = new ColorFader(Color.white, new Color(0.9607843f, 0.9607843f, 0.9607843f), new Color(0.7843137f, 0.7843137f, 0.7843137f), new Color(0.9607843f, 0.9607843f, 0.9607843f), new Color(0.94117647f, 0.94117647f, 0.94117647f), null, fillImage, 0.1f);
            Image bgImage = slider.transform.GetChild(0).GetComponent<Image>();
            ColorFader bgFader = new ColorFader(new Color(0.9843137f, 0.9843137f, 0.9843137f), new Color(0.94117647f, 0.94117647f, 0.94117647f), new Color(0.7843137f, 0.7843137f, 0.7843137f), new Color(0.94117647f, 0.94117647f, 0.94117647f), new Color(0.94117647f, 0.94117647f, 0.94117647f), null, bgImage, 0.1f);
            
            colors = new ColorFader[3]
            {
                handleFader,
                bgFader,
                fillAreaFader
            };
        }
    }

#endif

    #endregion
}
