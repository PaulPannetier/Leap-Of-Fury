using UnityEngine;

public class MapSelectorController : MonoBehaviour
{
    private TurningSelector mapSelector;
    private ControllerType controllerIndex;
    private bool isMapSelectorInit;

    [SerializeField] private ControllerSelector controllerSelector = ControllerSelector.last;

    private void Awake()
    {
        mapSelector = transform.GetChild(0).gameObject.GetComponent <TurningSelector>();
    }

    private void Start()
    {
        if(controllerSelector == ControllerSelector.keyboard || controllerSelector == ControllerSelector.gamepad1 || controllerSelector == ControllerSelector.gamepad2 || controllerSelector == ControllerSelector.gamepad3
            || controllerSelector == ControllerSelector.gamepad4 || controllerSelector == ControllerSelector.gamepadAll || controllerSelector == ControllerSelector.all)
        {
            isMapSelectorInit = true;
            controllerIndex = (ControllerType)controllerSelector;
        }
        else
        {
            isMapSelectorInit = false;
        }
    }

    private void Update()
    {
        if(isMapSelectorInit)
        {
            if (IsPressingRightOrLeft(controllerIndex, out bool right))
            {
                if (right)
                    mapSelector.SelectedNextItem();
                else
                    mapSelector.SelectPreviousItem();
            }
            if (IsApplyingSelection(controllerIndex))
            {
                TryLoadNextScene();
            }

            if(controllerSelector == ControllerSelector.last)
            {
                if (AnyKeyPressed(out ControllerType controllerType, out InputKey key))
                    controllerIndex = controllerType;
            }
        }
        else
        {
            if (AnyKeyPressed(out ControllerType controllerType, out InputKey key))
            {
                isMapSelectorInit = true;
                controllerIndex = controllerType;
            }
        }

        if (InputManager.GetGamepadUnPluggedAll(out ControllerType[] controllerTypes))
        {
            for (int i = 0; i < controllerTypes.Length; i++)
            {
                if (controllerTypes[i] == controllerIndex)
                {
                    isMapSelectorInit = false;
                }
            }
        }

        if (IsPressingEscape(controllerIndex))
        {
            TransitionManager.instance.LoadScene("Selection Char");
        }

        //DebugText.instance.text += mapSelector.selectedItem.GetComponent<MapSelectorItemData>().sceneName;
    }

    private void TryLoadNextScene()
    {
        object[] data = TransitionManager.instance.GetOldSceneData("Selection Char");
        MapSelectorItemData mapSelectorItemData = mapSelector.selectedItem.GetComponent<MapSelectorItemData>();
        TransitionManager.instance.LoadSceneAsync(mapSelectorItemData.sceneName, data);
    }

    private bool IsPressingEscape(in ControllerType controllerType)
    {
        switch (controllerType)
        {
            case ControllerType.Keyboard:
                return InputManager.GetKeyDown(KeyCode.Escape);
            case ControllerType.Gamepad1:
                return InputManager.GetKeyDown(KeyCode.Joystick1Button1);
            case ControllerType.Gamepad2:
                return InputManager.GetKeyDown(KeyCode.Joystick2Button1);
            case ControllerType.Gamepad3:
                return InputManager.GetKeyDown(KeyCode.Joystick3Button1);
            case ControllerType.Gamepad4:
                return InputManager.GetKeyDown(KeyCode.Joystick4Button1);
            case ControllerType.GamepadAll:
                return InputManager.GetKeyDown(KeyCode.Joystick1Button1) || InputManager.GetKeyDown(KeyCode.Joystick2Button1) || InputManager.GetKeyDown(KeyCode.Joystick3Button1) || InputManager.GetKeyDown(KeyCode.Joystick4Button1);
            case ControllerType.All:
                return InputManager.GetKeyDown(KeyCode.Escape) || InputManager.GetKeyDown(KeyCode.Joystick1Button1) || InputManager.GetKeyDown(KeyCode.Joystick2Button1) || InputManager.GetKeyDown(KeyCode.Joystick3Button1) || InputManager.GetKeyDown(KeyCode.Joystick4Button1);
            default:
                return false;
        }
    }

    #region IsApplyingSelection / PressingUpOrDown

    private bool IsApplyingSelection(in ControllerType controllerType)
    {
        switch (controllerType)
        {
            case ControllerType.Keyboard:
                return InputManager.GetKeyDown(KeyCode.Space) || InputManager.GetKeyDown(KeyCode.KeypadEnter) || InputManager.GetKeyDown(KeyCode.Return);
            case ControllerType.Gamepad1:
                return InputManager.GetKeyDown(KeyCode.Joystick1Button0);
            case ControllerType.Gamepad2:
                return InputManager.GetKeyDown(KeyCode.Joystick2Button0);
            case ControllerType.Gamepad3:
                return InputManager.GetKeyDown(KeyCode.Joystick3Button0);
            case ControllerType.Gamepad4:
                return InputManager.GetKeyDown(KeyCode.Joystick4Button0);
            case ControllerType.GamepadAll:
                return InputManager.GetKeyDown(KeyCode.Joystick1Button0) || InputManager.GetKeyDown(KeyCode.Joystick2Button0) ||
                    InputManager.GetKeyDown(KeyCode.Joystick3Button0) || InputManager.GetKeyDown(KeyCode.Joystick4Button0);
            case ControllerType.All:
                return InputManager.GetKeyDown(KeyCode.Space) || InputManager.GetKeyDown(KeyCode.KeypadEnter) || InputManager.GetKeyDown(KeyCode.Return) ||
                    InputManager.GetKeyDown(KeyCode.Joystick1Button0) || InputManager.GetKeyDown(KeyCode.Joystick2Button0) ||
                    InputManager.GetKeyDown(KeyCode.Joystick3Button0) || InputManager.GetKeyDown(KeyCode.Joystick4Button0);
            default:
                return false;
        }
    }

    private bool IsPressingRightOrLeft(ControllerType controllerType, out bool right)
    {
        bool TestControllerType(InputKey right, InputKey left, ControllerType controllerType, out bool b)
        {
            if (InputManager.GetKeyDown(right) || InputManager.GetGamepadStickRight(controllerType, GamepadStick.right) || InputManager.GetGamepadStickRight(controllerType, GamepadStick.left))
            {
                b = true;
                return true;
            }
            if (InputManager.GetKeyDown(left) || InputManager.GetGamepadStickLeft(controllerType, GamepadStick.right) || InputManager.GetGamepadStickLeft(controllerType, GamepadStick.left))
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
                if (InputManager.GetKeyDown(KeyCode.D) || InputManager.GetKeyDown(KeyCode.RightArrow))
                {
                    right = true;
                    return true;
                }
                if (InputManager.GetKeyDown(KeyCode.Q) || InputManager.GetKeyDown(KeyCode.LeftArrow))
                {
                    right = false;
                    return true;
                }
                right = false;
                return false;
            case ControllerType.Gamepad1:
                return TestControllerType(InputKey.GP1DPadRight, InputKey.GP1DPadLeft, ControllerType.Gamepad1, out right);
            case ControllerType.Gamepad2:
                return TestControllerType(InputKey.GP2DPadRight, InputKey.GP2DPadLeft, ControllerType.Gamepad2, out right);
            case ControllerType.Gamepad3:
                return TestControllerType(InputKey.GP3DPadRight, InputKey.GP3DPadLeft, ControllerType.Gamepad3, out right);
            case ControllerType.Gamepad4:
                return TestControllerType(InputKey.GP4DPadRight, InputKey.GP4DPadLeft, ControllerType.Gamepad4, out right);
            case ControllerType.GamepadAll:
                Debug.Log("Debug plz");
                right = false;
                return false;
            case ControllerType.All:
                Debug.Log("Debug plz");
                right = false;
                return false;
            default:
                right = false;
                return false;
        }
    }

    #endregion

    #region ControllerAlreadyInit / NewCOntrollerPress a key

    private bool AnyKeyPressed(out ControllerType controllerType, out InputKey key)
    {
        bool TestControllerType(ControllerType controllerType, out InputKey key)
        {
            if (controllerType == ControllerType.Keyboard || InputManager.IsGamePadConnected(controllerType))
            {
                if (InputManager.Listen(controllerType, out key))
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

    #endregion
}
