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

#if UNITY_EDITOR
    public bool generateDefaultSliderColorFaders = false;
#endif

    public override void OnPressed()
    {
        if(isSelected)
        {
            print("OnPressed");
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
            print("Decrease");
            slider.value = Mathf.Max(0f, slider.value - (Time.deltaTime / durationToFill));
        }

        if(inputIncrease.IsPressed())
        {
            print("Increase");
            slider.value = Mathf.Min(1f, slider.value + (Time.deltaTime / durationToFill));
        }
    }

    #region OnValidate

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        durationToFill = Mathf.Max(0f, durationToFill);

        if(generateDefaultSliderColorFaders)
        {
            generateDefaultSliderColorFaders = false;

            Image handleImage = slider.transform.GetChild(2).GetChild(0).GetComponent<Image>();
            ColorFader handleFader = new ColorFader(Color.white, new Color(0.9607843f, 0.9607843f, 0.9607843f), new Color(0.7843137f, 0.7843137f, 0.7843137f), new Color(0.9607843f, 0.9607843f, 0.9607843f), null, handleImage, 0.1f);
            Image fillImage = slider.transform.GetChild(1).GetChild(0).GetComponent<Image>();
            ColorFader fillAreaFader = new ColorFader(Color.white, new Color(0.9607843f, 0.9607843f, 0.9607843f), new Color(0.7843137f, 0.7843137f, 0.7843137f), new Color(0.9607843f, 0.9607843f, 0.9607843f), null, fillImage, 0.1f);
            Image bgImage = slider.transform.GetChild(0).GetComponent<Image>();
            ColorFader bgFader = new ColorFader(new Color(0.9843137f, 0.9843137f, 0.9843137f), new Color(0.94117647f, 0.94117647f, 0.94117647f), new Color(0.7843137f, 0.7843137f, 0.7843137f), new Color(0.94117647f, 0.94117647f, 0.94117647f), null, bgImage, 0.1f);
            
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
