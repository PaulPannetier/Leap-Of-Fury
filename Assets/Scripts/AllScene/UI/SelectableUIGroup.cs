using UnityEngine;

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
    [SerializeField] private ControllerSelector controllerSelector = ControllerSelector.last;
    [SerializeField] private SelectableUI defaultUISelected;
    [SerializeField] private InputManager.GeneralInput upItemInput;
    [SerializeField] private InputManager.GeneralInput downItemInput;
    [SerializeField] private InputManager.GeneralInput rightItemInput;
    [SerializeField] private InputManager.GeneralInput leftItemInput;
    [SerializeField] private InputManager.GeneralInput applyInput;

    private void Awake()
    {
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
                switch (controllerSelector)
                {
                    case ControllerSelector.keyboard:
                        controllerType = ControllerType.Keyboard;
                        break;
                    case ControllerSelector.gamepad1:
                        controllerType = ControllerType.Gamepad1;
                        break;
                    case ControllerSelector.gamepad2:
                        controllerType = ControllerType.Gamepad2;
                        break;
                    case ControllerSelector.gamepad3:
                        controllerType = ControllerType.Gamepad3;
                        break;
                    case ControllerSelector.gamepad4:
                        controllerType = ControllerType.Gamepad4;
                        break;
                    case ControllerSelector.gamepadAll:
                        controllerType = ControllerType.GamepadAll;
                        break;
                    case ControllerSelector.all:
                        controllerType = ControllerType.All;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                if(ControllerIsPressingAKey(out ControllerType controllerType, out InputKey key))
                {
                    selectedUI = defaultUISelected;
                    this.controllerType = controllerType;
                }
            }
        }
        else
        {
            if (controllerSelector == ControllerSelector.last)
            {
                if (ControllerIsPressingAKey(out ControllerType controllerType, out InputKey key))
                    this.controllerType = controllerType;
            }

            if(selectedUI.upSelectableUI != null && upItemInput.IsPressedDown())
            {
                selectedUI.isSelected = false;
                selectedUI.upSelectableUI.isSelected = true;
                selectedUI = selectedUI.upSelectableUI;
            }

            if (selectedUI.downSelectableUI != null && downItemInput.IsPressedDown())
            {
                selectedUI.isSelected = false;
                selectedUI.downSelectableUI.isSelected = true;
                selectedUI = selectedUI.downSelectableUI;
            }

            if (selectedUI.rightSelectableUI != null && rightItemInput.IsPressedDown())
            {
                selectedUI.isSelected = false;
                selectedUI.rightSelectableUI.isSelected = true;
                selectedUI = selectedUI.rightSelectableUI;
            }

            if (selectedUI.leftSelectableUI != null && leftItemInput.IsPressedDown())
            {
                selectedUI.isSelected = false;
                selectedUI.leftSelectableUI.isSelected = true;
                selectedUI = selectedUI.leftSelectableUI;
            }

            //validate
            if (applyInput.IsPressedDown())
            {
                selectedUI.OnPressed();
            }
        }
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
