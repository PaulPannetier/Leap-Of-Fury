using System.Collections;
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
                if (AnyKeyPressed(out ControllerType controllerType, out int key))
                    controllerIndex = controllerType;
            }
        }
        else
        {
            if (AnyKeyPressed(out ControllerType controllerType, out int key))
            {
                isMapSelectorInit = true;
                controllerIndex = controllerType;
            }
        }

        if (CustomInput.GetGamepadUnPluggedAll(out ControllerType[] controllerTypes))
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

        DebugText.instance.text += mapSelector.selectedItem.GetComponent<MapSelectorItemData>().sceneName;
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
                return CustomInput.GetKeyDown(KeyCode.Escape);
            case ControllerType.Gamepad1:
                return CustomInput.GetKeyDown(KeyCode.Joystick1Button1);
            case ControllerType.Gamepad2:
                return CustomInput.GetKeyDown(KeyCode.Joystick2Button1);
            case ControllerType.Gamepad3:
                return CustomInput.GetKeyDown(KeyCode.Joystick3Button1);
            case ControllerType.Gamepad4:
                return CustomInput.GetKeyDown(KeyCode.Joystick4Button1);
            case ControllerType.GamepadAll:
                return CustomInput.GetKeyDown(KeyCode.Joystick1Button1) || CustomInput.GetKeyDown(KeyCode.Joystick2Button1) || CustomInput.GetKeyDown(KeyCode.Joystick3Button1) || CustomInput.GetKeyDown(KeyCode.Joystick4Button1);
            case ControllerType.All:
                return CustomInput.GetKeyDown(KeyCode.Escape) || CustomInput.GetKeyDown(KeyCode.Joystick1Button1) || CustomInput.GetKeyDown(KeyCode.Joystick2Button1) || CustomInput.GetKeyDown(KeyCode.Joystick3Button1) || CustomInput.GetKeyDown(KeyCode.Joystick4Button1);
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
                return CustomInput.GetKeyDown(KeyCode.Space) || CustomInput.GetKeyDown(KeyCode.KeypadEnter) || CustomInput.GetKeyDown(KeyCode.Return);
            case ControllerType.Gamepad1:
                return CustomInput.GetKeyDown(KeyCode.Joystick1Button0);
            case ControllerType.Gamepad2:
                return CustomInput.GetKeyDown(KeyCode.Joystick2Button0);
            case ControllerType.Gamepad3:
                return CustomInput.GetKeyDown(KeyCode.Joystick3Button0);
            case ControllerType.Gamepad4:
                return CustomInput.GetKeyDown(KeyCode.Joystick4Button0);
            case ControllerType.GamepadAll:
                return CustomInput.GetKeyDown(KeyCode.Joystick1Button0) || CustomInput.GetKeyDown(KeyCode.Joystick2Button0) ||
                    CustomInput.GetKeyDown(KeyCode.Joystick3Button0) || CustomInput.GetKeyDown(KeyCode.Joystick4Button0);
            case ControllerType.All:
                return CustomInput.GetKeyDown(KeyCode.Space) || CustomInput.GetKeyDown(KeyCode.KeypadEnter) || CustomInput.GetKeyDown(KeyCode.Return) ||
                    CustomInput.GetKeyDown(KeyCode.Joystick1Button0) || CustomInput.GetKeyDown(KeyCode.Joystick2Button0) ||
                    CustomInput.GetKeyDown(KeyCode.Joystick3Button0) || CustomInput.GetKeyDown(KeyCode.Joystick4Button0);
            default:
                return false;
        }
    }

    private bool IsPressingRightOrLeft(in ControllerType controllerType, out bool right)
    {
        bool TestControllerType(in NegativeKeyCode right, in NegativeKeyCode left, in ControllerType controllerType, out bool b)
        {
            if (CustomInput.GetKeyDown(right) || CustomInput.GetGamepadStickRight(controllerType, GamepadStick.right) || CustomInput.GetGamepadStickRight(controllerType, GamepadStick.left))
            {
                b = true;
                return true;
            }
            if (CustomInput.GetKeyDown(left) || CustomInput.GetGamepadStickLeft(controllerType, GamepadStick.right) || CustomInput.GetGamepadStickLeft(controllerType, GamepadStick.left))
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
                if (CustomInput.GetKeyDown(KeyCode.D) || CustomInput.GetKeyDown(KeyCode.RightArrow))
                {
                    right = true;
                    return true;
                }
                if (CustomInput.GetKeyDown(KeyCode.Q) || CustomInput.GetKeyDown(KeyCode.LeftArrow))
                {
                    right = false;
                    return true;
                }
                right = false;
                return false;
            case ControllerType.Gamepad1:
                return TestControllerType(NegativeKeyCode.GP1DPadRight, NegativeKeyCode.GP1DPadLeft, ControllerType.Gamepad1, out right);
            case ControllerType.Gamepad2:
                return TestControllerType(NegativeKeyCode.GP2DPadRight, NegativeKeyCode.GP2DPadLeft, ControllerType.Gamepad2, out right);
            case ControllerType.Gamepad3:
                return TestControllerType(NegativeKeyCode.GP3DPadRight, NegativeKeyCode.GP3DPadLeft, ControllerType.Gamepad3, out right);
            case ControllerType.Gamepad4:
                return TestControllerType(NegativeKeyCode.GP4DPadRight, NegativeKeyCode.GP4DPadLeft, ControllerType.Gamepad4, out right);
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

    private bool AnyKeyPressed(out ControllerType controllerType, out int key)
    {
        bool TestControllerType(in ControllerType controllerType, out int key)
        {
            if (controllerType == ControllerType.Keyboard || CustomInput.GamePadIsConnected(controllerType))
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

    #endregion
}
