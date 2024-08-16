using UnityEngine;
using System.Collections.Generic;

public enum PlayerSelector
{
    player1 = 1,
    player2 = 2,
    player3 = 3,
    player4 = 4,
    player5 = 5,
    first = 6,
    last = 7,
    all = 8
}

public enum ControllerSelector
{
    keyboard = 0,
    gamepad1 = 1,
    gamepad2 = 2,
    gamepad3 = 3,
    gamepad4 = 4,
    gamepadAll = 5,
    all = 6,
    first = 7,
    last = 8
}

public class SelectableUIGroup : MonoBehaviour
{
    private SelectableUI selectedUI = null;
    private ControllerType _controllerType;
    private ControllerType controllerType
    {
        get => _controllerType;
        set
        {
            _controllerType = value;
            upItemInput.controllerType = value;
            downItemInput.controllerType = value;
            rightItemInput.controllerType = value;
            leftItemInput.controllerType = value;
            applyInput.controllerType = value;
        }
    }

    public bool enableBehaviour = true;

    [SerializeField] private BaseController allowedController = BaseController.KeyboardAndGamepad;
    [SerializeField] private ControllerSelector controllerSelector = ControllerSelector.last;
    [SerializeField] private SelectableUI defaultUISelected;
    [SerializeField] private InputManager.GeneralInput upItemInput;
    [SerializeField] private InputManager.GeneralInput downItemInput;
    [SerializeField] private InputManager.GeneralInput rightItemInput;
    [SerializeField] private InputManager.GeneralInput leftItemInput;
    [SerializeField] private InputManager.GeneralInput applyInput;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        selectedUI = null;

        HashSet<SelectableUI> cache = new HashSet<SelectableUI>();
        InitRecur(defaultUISelected, ref cache);

        void InitRecur(SelectableUI selectableUI, ref HashSet<SelectableUI> cache)
        {
            if (cache.Contains(selectableUI))
                return;

            selectableUI.selectableUIGroup = this;
            selectableUI.isMouseInteractable = allowedController != BaseController.Gamepad;
            selectableUI.OnDeselected();
            cache.Add(selectableUI);
            if (selectableUI.upSelectableUI != null)
                InitRecur(selectableUI.upSelectableUI, ref cache);
            if (selectableUI.downSelectableUI != null)
                InitRecur(selectableUI.downSelectableUI, ref cache);
            if (selectableUI.rightSelectableUI != null)
                InitRecur(selectableUI.rightSelectableUI, ref cache);
            if (selectableUI.leftSelectableUI != null)
                InitRecur(selectableUI.leftSelectableUI, ref cache);
        }
    }

    public void ResetToDefault()
    {
        HashSet<SelectableUI> cache = new HashSet<SelectableUI>();
        ResetSelectableUIRecur(defaultUISelected, ref cache);

        void ResetSelectableUIRecur(SelectableUI selectableUI, ref HashSet<SelectableUI> cache)
        {
            if (cache.Contains(selectableUI))
                return;

            selectableUI.ResetToDefault();
            cache.Add(selectableUI);
            if (selectableUI.upSelectableUI != null)
                ResetSelectableUIRecur(selectableUI.upSelectableUI, ref cache);
            if (selectableUI.downSelectableUI != null)
                ResetSelectableUIRecur(selectableUI.downSelectableUI, ref cache);
            if (selectableUI.rightSelectableUI != null)
                ResetSelectableUIRecur(selectableUI.rightSelectableUI, ref cache);
            if (selectableUI.leftSelectableUI != null)
                ResetSelectableUIRecur(selectableUI.leftSelectableUI, ref cache);
        }
    }

    public void RequestSelected(SelectableUI selectableUI)
    {
        if (selectedUI != null)
        {
            selectedUI.OnDeselected();    
        }
        selectedUI = selectableUI;
    }

    public void RequestDeselected(SelectableUI selectableUI)
    {
        if(selectedUI != selectableUI)
        {
            selectedUI.OnDeselected();
        }
        selectedUI = null;
    }

    private void Update()
    {
        if (!enableBehaviour)
            return;

        //on attend la première interaction
        if(selectedUI == null)
        {
            if(controllerSelector == ControllerSelector.keyboard || controllerSelector == ControllerSelector.gamepad1 || controllerSelector == ControllerSelector.gamepad2 || controllerSelector == ControllerSelector.gamepad3
                 || controllerSelector == ControllerSelector.gamepad4 || controllerSelector == ControllerSelector.gamepadAll || controllerSelector == ControllerSelector.all)
            {
                selectedUI = defaultUISelected;
                controllerType = (ControllerType)controllerSelector;
            }
            else
            {
                if(ControllerIsPressingAKey(out ControllerType controllerType, out InputKey key))
                {
                    if(IsControllerTypeAnAllowedController(controllerType))
                    {
                        selectedUI = defaultUISelected;
                        selectedUI.OnSelected();
                        this.controllerType = controllerType;
                    }
                }
            }
        }
        else
        {
            bool changeControllerType = false;
            if (controllerSelector == ControllerSelector.last)
            {
                if (ControllerIsPressingAKey(out ControllerType controllerType, out InputKey key))
                {
                    if(this.controllerType != controllerType && IsControllerTypeAnAllowedController(controllerType))
                    {
                        this.controllerType = controllerType;
                        changeControllerType = true;
                    }
                }
            }

            if(!changeControllerType)
            {
                if (selectedUI.upSelectableUI != null && upItemInput.IsPressedDown())
                {
                    selectedUI.OnDeselected();
                    selectedUI = selectedUI.upSelectableUI;
                    selectedUI.OnSelected();
                }

                if (selectedUI.downSelectableUI != null && downItemInput.IsPressedDown())
                {
                    selectedUI.OnDeselected();
                    selectedUI = selectedUI.downSelectableUI;
                    selectedUI.OnSelected();
                }

                if (selectedUI.rightSelectableUI != null && rightItemInput.IsPressedDown())
                {
                    selectedUI.OnDeselected();
                    selectedUI = selectedUI.rightSelectableUI;
                    selectedUI.OnSelected();
                }

                if (selectedUI.leftSelectableUI != null && leftItemInput.IsPressedDown())
                {
                    selectedUI.OnDeselected();
                    selectedUI = selectedUI.leftSelectableUI;
                    selectedUI.OnSelected();
                }

                //validate
                if (applyInput.IsPressedDown())
                {
                    selectedUI.OnPressed();
                }
            }
        }
    }

    private bool IsControllerTypeAnAllowedController(ControllerType controllerType)
    {
        if (allowedController == BaseController.KeyboardAndGamepad || controllerType == ControllerType.All)
            return true;

        if (controllerType == ControllerType.Keyboard && allowedController == BaseController.Keyboard)
            return true;

        return allowedController == BaseController.Gamepad && controllerType != ControllerType.Keyboard;
    }

    private bool ControllerIsPressingAKey(out ControllerType controllerType, out InputKey key)
    {
        bool TestControllerType(ControllerType controllerType, out InputKey key)
        {
            if ((controllerType == ControllerType.Keyboard || InputManager.IsGamePadConnected(controllerType)))
            {
                if (InputManager.Listen(controllerType, out key))
                    return true;
            }
            key = InputKey.None;
            return false;
        }

        if (TestControllerType(ControllerType.Keyboard, out key))
        {
            controllerType = ControllerType.Keyboard;
            return true;
        }
        if (TestControllerType(ControllerType.Gamepad1, out key))
        {
            controllerType = ControllerType.Gamepad1;
            return true;
        }
        if (TestControllerType(ControllerType.Gamepad2, out key))
        {
            controllerType = ControllerType.Gamepad2;
            return true;
        }
        if (TestControllerType(ControllerType.Gamepad3, out key))
        {
            controllerType = ControllerType.Gamepad3;
            return true;
        }
        if (TestControllerType(ControllerType.Gamepad4, out key))
        {
            controllerType = ControllerType.Gamepad4;
            return true;
        }
        key = (int)KeyCode.None;
        controllerType = ControllerType.Keyboard;
        return false;
    }
}
