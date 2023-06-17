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
    private ControllerType controllerType;

    public bool enableBehaviour = true;
    [SerializeField] private ControllerSelector controllerSelector = ControllerSelector.last;
    [SerializeField] private SelectableUI defaultUISelected;

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
            }
            else
            {
                if(ControllerIsPressingAKey(out ControllerType controllerType, out int key))
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
                if (ControllerIsPressingAKey(out ControllerType controllerType, out int key))
                    this.controllerType = controllerType;
            }
            if (selectedUI.upSelectableUI != null && IsPressingUpOrDown(controllerType, out bool up))
            {
                selectedUI.isSelected = false;
                selectedUI = up ? selectedUI.upSelectableUI : selectedUI.downSelectableUI;
                selectedUI.isSelected = true;
            }

            //validate
            if(IsPressedApply(controllerType))
            {
                selectedUI.OnPressed();
            }
        }
    }

    private bool ControllerIsPressingAKey(out ControllerType controllerType, out int key)
    {
        bool TestControllerType(in ControllerType controllerType, out int key)
        {
            if ((controllerType == ControllerType.Keyboard || CustomInput.GamePadIsConnected(controllerType)))
            {
                if (CustomInput.Listen(controllerType, out key))
                    return true;
            }
            key = (int)KeyCode.None;
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

    private bool IsPressedApply(in ControllerType controllerType)
    {
        switch (controllerType)
        {
            case ControllerType.Keyboard:
                return CustomInput.GetKeyDown(KeyCode.Space) || CustomInput.GetKeyDown(KeyCode.Return);
            case ControllerType.Gamepad1:
                return CustomInput.GetKeyDown(KeyCode.Joystick1Button0);
            case ControllerType.Gamepad2:
                return CustomInput.GetKeyDown(KeyCode.Joystick2Button0);
            case ControllerType.Gamepad3:
                return CustomInput.GetKeyDown(KeyCode.Joystick3Button0);
            case ControllerType.Gamepad4:
                return CustomInput.GetKeyDown(KeyCode.Joystick4Button0);
            case ControllerType.GamepadAll:
                return CustomInput.GetKeyDown(KeyCode.Joystick1Button0) || CustomInput.GetKeyDown(KeyCode.Joystick2Button0) || CustomInput.GetKeyDown(KeyCode.Joystick3Button0) || CustomInput.GetKeyDown(KeyCode.Joystick4Button0);
            case ControllerType.All:
                return CustomInput.GetKeyDown(KeyCode.Space) || CustomInput.GetKeyDown(KeyCode.KeypadEnter) || CustomInput.GetKeyDown(KeyCode.Joystick1Button0) ||
                    CustomInput.GetKeyDown(KeyCode.Joystick2Button0) || CustomInput.GetKeyDown(KeyCode.Joystick3Button0) || CustomInput.GetKeyDown(KeyCode.Joystick4Button0);
            default:
                return false;
        }
    }

    private bool IsPressingUpOrDown(in ControllerType controllerType, out bool up)
    {
        bool TestControllerType(in NegativeKeyCode up, in NegativeKeyCode down, in ControllerType controllerType, out bool b)
        {
            if (CustomInput.GetKeyDown(up) || CustomInput.GetGamepadStickUp(controllerType, GamepadStick.right) || CustomInput.GetGamepadStickUp(controllerType, GamepadStick.left))
            {
                b = true;
                return true;
            }
            if (CustomInput.GetKeyDown(down) || CustomInput.GetGamepadStickDown(controllerType, GamepadStick.right) || CustomInput.GetGamepadStickDown(controllerType, GamepadStick.left))
            {
                b = false;
                return true;
            }
            b = false;
            return false;
        }

        switch (controllerType)
        {
            case ControllerType.Keyboard:
                if (CustomInput.GetKeyDown(KeyCode.W) || CustomInput.GetKeyDown(KeyCode.UpArrow))
                {
                    up = true;
                    return true;
                }
                if (CustomInput.GetKeyDown(KeyCode.S) || CustomInput.GetKeyDown(KeyCode.DownArrow))
                {
                    up = false;
                    return true;
                }
                up = false;
                return false;
            case ControllerType.Gamepad1:
                return TestControllerType(NegativeKeyCode.GP1DPadUp, NegativeKeyCode.GP1DPadDown, ControllerType.Gamepad1, out up);
            case ControllerType.Gamepad2:
                return TestControllerType(NegativeKeyCode.GP2DPadUp, NegativeKeyCode.GP2DPadDown, ControllerType.Gamepad2, out up);
            case ControllerType.Gamepad3:
                return TestControllerType(NegativeKeyCode.GP3DPadUp, NegativeKeyCode.GP3DPadDown, ControllerType.Gamepad3, out up);
            case ControllerType.Gamepad4:
                return TestControllerType(NegativeKeyCode.GP4DPadUp, NegativeKeyCode.GP4DPadDown, ControllerType.Gamepad4, out up);
            case ControllerType.GamepadAll:
                Debug.Log("Debug plz");
                up = false;
                return false;
            case ControllerType.All:
                Debug.Log("Debug plz");
                up = false;
                return false;
            default:
                up = false;
                return false;
        }
    }
}
