using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DropDownSelectableUI : SelectableUI
{
    private bool isActivatedThisFrame = false;
    private InputManager.GeneralInput downInput, upInput;
    private int initValue, currentValue;

    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private InputManager.GeneralInput applyInput;
    [SerializeField] private InputManager.GeneralInput desactivateInput;

    public override SelectableUIGroup selectableUIGroup
    {
        get => base.selectableUIGroup;
        set
        {
            base.selectableUIGroup = value;
            if(value != null)
            {
                ControllerType controllerType = ControllerType.Keyboard;
                if (value.allowedController == BaseController.KeyboardAndGamepad)
                    controllerType = ControllerType.All;
                else if (value.allowedController == BaseController.Gamepad)
                    controllerType = ControllerType.GamepadAll;

                downInput.controllerType = upInput.controllerType = applyInput.controllerType = desactivateInput.controllerType = controllerType;
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        downInput.keysKeyboard = new KeyboardKey[1] { KeyboardKey.UpArrow };
        downInput.keyGamepad1 = new GamepadKey[1] { GamepadKey.GP1TBSLUp };
        downInput.keyGamepad2 = new GamepadKey[1] { GamepadKey.GP2TBSLUp };
        downInput.keyGamepad3 = new GamepadKey[1] { GamepadKey.GP3TBSLUp };
        downInput.keyGamepad4 = new GamepadKey[1] { GamepadKey.GP4TBSLUp };

        upInput.keysKeyboard = new KeyboardKey[1] { KeyboardKey.DownArrow };
        upInput.keyGamepad1 = new GamepadKey[1] { GamepadKey.GP1TBSLDown };
        upInput.keyGamepad2 = new GamepadKey[1] { GamepadKey.GP2TBSLDown };
        upInput.keyGamepad3 = new GamepadKey[1] { GamepadKey.GP3TBSLDown };
        upInput.keyGamepad4 = new GamepadKey[1] { GamepadKey.GP4TBSLDown };
    }

    private void AdjustScrollPosition()
    {
        if (dropdown.transform.Find("Dropdown List"))
        {
            // Get the ScrollRect component (which contains the scrollbar and viewport)
            ScrollRect scrollRect = dropdown.transform.Find("Dropdown List").GetComponentInChildren<ScrollRect>();

            if (scrollRect != null)
            {
                // Calculate the relative position of the selected item (normalized between 0 and 1)
                float normalizedIndexPosition = (float)currentValue / (dropdown.options.Count - 1f);
                // Smoothly move the scrollbar to the selected item
                scrollRect.verticalNormalizedPosition = 1f - Mathf.Clamp01(normalizedIndexPosition);
            }
        }
    }

    public override void OnPressed()
    {
        if (isSelected && !isDesactivatedThisFrame && !isActivatedThisFrame)
        {
            isActive = isActivatedThisFrame = true;
            dropdown.Show();
            initValue = currentValue = dropdown.value;
        }
    }

    private void Update()
    {
        isDesactivatedThisFrame = false;

        if (!isActive)
            return;

        dropdown.RefreshShownValue();

        if (desactivateInput.IsPressedDown() && !isActivatedThisFrame)
        {
            dropdown.value = initValue;
            Desactivate();
        }

        if (applyInput.IsPressedDown() && !isActivatedThisFrame)
        {
            dropdown.RefreshShownValue();
            Desactivate();
        }

        if(upInput.IsPressedDown())
        {
            currentValue = Mathf.Min(0, currentValue - 1);
            AdjustScrollPosition();
        }

        if(downInput.IsPressedDown())
        {
            currentValue = Mathf.Max(dropdown.options.Count - 1, currentValue + 1);
            AdjustScrollPosition();
        }

        isActivatedThisFrame = false;

        void Desactivate()
        {
            isActive = false;
            isDesactivatedThisFrame = true;
            dropdown.Hide();
            //This lane is terrible, can't UnSelect the TMP_Dropdown so select a random gameobject to unselect the dropdown
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}
