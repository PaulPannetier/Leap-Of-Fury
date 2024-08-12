using UnityEngine;
using UnityEngine.UI;

public class CheckboxSelectableUI : SelectableUI
{
    private bool isActive = false;

    [SerializeField] private Toggle toggle;
    [SerializeField] private InputManager.GeneralInput toggleInput;
    [SerializeField] private InputManager.GeneralInput desactivateInput;

    public override void OnPressed()
    {
        if (isSelected)
        {
            selectableUIGroup.enableBehaviour = false;
            isActive = true;
        }
    }

    private void Update()
    {
        if (!isActive)
            return;

        if (desactivateInput.IsPressedDown())
        {
            isActive = false;
            selectableUIGroup.enableBehaviour = true;
        }

        if(toggleInput.IsPressedDown())
        {
            toggle.isOn = !toggle.isOn;
        }
    }
}
