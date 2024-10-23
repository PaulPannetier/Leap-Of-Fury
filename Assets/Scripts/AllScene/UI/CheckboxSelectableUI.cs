using UnityEngine;
using UnityEngine.UI;

public class CheckboxSelectableUI : SelectableUI
{
    [SerializeField] private Toggle toggle;

    public bool isOn
    {
        get => toggle.isOn;
        set => toggle.isOn = value;
    }

    public override bool interactable
    {
        get => base.interactable;
        set
        {
            base.interactable = value;
            toggle.interactable = value;
        }
    }

    public override void OnPressedUp()
    {

    }

    public override void OnPressed()
    {
        if (isSelected)
        {
            toggle.isOn = !toggle.isOn;
        }
    }

    #region OnValidate

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        if (toggle == null)
        {
            toggle = GetComponentInChildren<Toggle>();
        }
    }

#endif

    #endregion
}
