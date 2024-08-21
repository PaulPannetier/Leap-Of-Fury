using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class DropDownSelectableUI : SelectableUI
{
    private const float delayBetweenStartHoldingAndRepeateKey = 0.5f;
    private const float delayBetweenRepeatingKey = 0.1f;

    private bool isActivatedThisFrame = false;
    private InputManager.GeneralInput downInput, upInput;
    private int initValue, currentValue;
    private float lastTimeHoldChangeKey = -10f;
    private bool isDelayBetweenStartHoldingAndRepeateKeyPass;

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
        upInput.keysKeyboard = new KeyboardKey[1] { KeyboardKey.UpArrow };
        upInput.keyGamepad1 = new GamepadKey[1] { GamepadKey.GP1TBSLUp };
        upInput.keyGamepad2 = new GamepadKey[1] { GamepadKey.GP2TBSLUp };
        upInput.keyGamepad3 = new GamepadKey[1] { GamepadKey.GP3TBSLUp };
        upInput.keyGamepad4 = new GamepadKey[1] { GamepadKey.GP4TBSLUp };

        downInput.keysKeyboard = new KeyboardKey[1] { KeyboardKey.DownArrow };
        downInput.keyGamepad1 = new GamepadKey[1] { GamepadKey.GP1TBSLDown };
        downInput.keyGamepad2 = new GamepadKey[1] { GamepadKey.GP2TBSLDown };
        downInput.keyGamepad3 = new GamepadKey[1] { GamepadKey.GP3TBSLDown };
        downInput.keyGamepad4 = new GamepadKey[1] { GamepadKey.GP4TBSLDown };
    }

    private void AdjustScrollPosition()
    {
        float normalizedIndexPosition = currentValue / (dropdown.options.Count - 1f);
        SetExtendedDropdownPosition(normalizedIndexPosition);
    }

    private void SetExtendedDropdownPosition(float position)
    {
        if (dropdown.transform.Find("Dropdown List"))
        {
            ScrollRect scrollRect = dropdown.transform.Find("Dropdown List").GetComponentInChildren<ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f - Mathf.Clamp01(position);
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

    private void IncreaseCurrentValue()
    {
        currentValue = Mathf.Min(dropdown.options.Count - 1, currentValue + 1);
        AdjustScrollPosition();
        lastTimeHoldChangeKey = Time.time;
    }

    private void DecreaseCurrentValue()
    {
        currentValue = Mathf.Max(0, currentValue - 1);
        AdjustScrollPosition();
        lastTimeHoldChangeKey = Time.time;
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
            DecreaseCurrentValue();
        }

        if(downInput.IsPressedDown())
        {
            IncreaseCurrentValue();
        }

        void HandleReapitingKey(Action action)
        {
            if (isDelayBetweenStartHoldingAndRepeateKeyPass)
            {
                if (Time.time - lastTimeHoldChangeKey > delayBetweenRepeatingKey)
                {
                    action.Invoke();
                }
            }
            else
            {
                if (Time.time - lastTimeHoldChangeKey > delayBetweenStartHoldingAndRepeateKey)
                {
                    isDelayBetweenStartHoldingAndRepeateKeyPass = true;
                    action.Invoke();
                }
            }
        }

        if(upInput.IsPressed())
        {
            HandleReapitingKey(DecreaseCurrentValue);
        }

        if (downInput.IsPressed())
        {
            HandleReapitingKey(IncreaseCurrentValue);
        }

        if (upInput.IsPressedUp() || downInput.IsPressedUp())
        {
            lastTimeHoldChangeKey = -10f;
            isDelayBetweenStartHoldingAndRepeateKeyPass = false;
        }

        isActivatedThisFrame = false;

        void Desactivate()
        {
            isActive = false;
            isDesactivatedThisFrame = true;
            dropdown.Hide();
            //This line is terrible, can't UnSelect the TMP_Dropdown so select a random gameobject to unselect the dropdown
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}
