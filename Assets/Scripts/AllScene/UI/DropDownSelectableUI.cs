using UnityEngine;
using TMPro;

public class DropDownSelectableUI : SelectableUI
{
    private bool isActive = false;
    private int initValue;

    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private InputManager.GeneralInput inputUp;
    [SerializeField] private InputManager.GeneralInput inputDown;
    [SerializeField] private InputManager.GeneralInput applyInput;
    [SerializeField] private InputManager.GeneralInput desactivateInput;

    public override void OnPressed()
    {
        if (isSelected)
        {
            selectableUIGroup.enableBehaviour = false;
            isActive = true;
            initValue = dropdown.value;
        }
    }

    private void Update()
    {
        if (!isActive)
            return;

        if (desactivateInput.IsPressedDown())
        {
            dropdown.value = initValue;
            isActive = false;
            selectableUIGroup.enableBehaviour = true;
        }

        if (applyInput.IsPressedDown())
        {
            isActive = false;
            selectableUIGroup.enableBehaviour = true;
        }

        if (inputUp.IsPressedDown())
        {
            dropdown.value = (dropdown.value + 1) % dropdown.options.Count;
            dropdown.RefreshShownValue();
        }

        if (inputDown.IsPressedDown())
        {
            dropdown.value = (dropdown.value - 1 + dropdown.options.Count) % dropdown.options.Count;
            dropdown.RefreshShownValue();
        }
    }
}
