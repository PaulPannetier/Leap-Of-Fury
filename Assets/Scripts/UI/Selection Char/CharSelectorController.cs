using System;
using System.Collections;
using UnityEngine;

public class CharSelectorController : MonoBehaviour
{
    private TurningSelector[] turningSelectors;
    private GameObject[] helpCanvas;
    private ControllerType[] controllerIndexs;
    private bool[] isTurningSelectorInit;
    private bool[] isTurningSlectorsFinishSelection;
    private bool[] isHelpCanvasOpen;
    private int indexToInit;
    private bool canLoadNextScene = false;
    private bool nextSceneIsLoading = false;

    [SerializeField] private GameObject[] charHelperCanvasPrefabs;

    [SerializeField] private InputManager.GeneralInput helpButton;
    [SerializeField] private InputManager.GeneralInput escapeButton;

    private void Awake()
    {
        turningSelectors = new TurningSelector[4];
        for (int i = 0; i < 4; i++)
        {
            turningSelectors[i] = transform.GetChild(i).gameObject.GetComponent<TurningSelector>();
        }
    }

    private void Start()
    {
        isTurningSelectorInit = new bool[4] { false, false, false, false };
        isTurningSlectorsFinishSelection = new bool[4] { false, false, false, false };
        isHelpCanvasOpen = new bool[4] { false, false, false, false };
        helpCanvas = new GameObject[4];
        controllerIndexs = new ControllerType[4];
        indexToInit = 0;
    }

    private void Update()
    {
        for (int i = 0; i < turningSelectors.Length; i++)
        {
            if (!isTurningSelectorInit[i])
                break;

            if (isHelpCanvasOpen[i])
                continue;

            if(IsPressingUpOrDown(controllerIndexs[i], out bool up))
            {
                if(up)
                {
                    turningSelectors[i].SelectedNextItem();
                }
                else
                {
                    turningSelectors[i].SelectPreviousItem();
                }
            }
            if(IsApplyingSelection(controllerIndexs[i]))
            {
                isTurningSlectorsFinishSelection[i] = true;
            }
            else if(IsUnApplyingSelection(controllerIndexs[i]))
            {
                isTurningSlectorsFinishSelection[i] = false;
            }
        }

        if (InputManager.GetGamepadUnPluggedAll(out ControllerType[] controllerTypes))
        {
            for (int i = 0; i < controllerTypes.Length; i++)
            {
                if (ControllerIsAlreadyInit(controllerTypes[i], out int index))
                {
                    RemoveSettingsIndex(index);
                }
            }
        }

        //si il reste un char a init
        if (!isTurningSelectorInit[turningSelectors.Length - 1])
        {
            if (NewControllerIsPressingAKey(out ControllerType controllerType, out InputKey key))
            {
                isTurningSelectorInit[indexToInit] = true;
                controllerIndexs[indexToInit] = controllerType;
                indexToInit++;
            }
        }

        bool allIsSelected = true;
        for (int i = 0; i < turningSelectors.Length; i++)
        {
            if(isTurningSelectorInit[i] && !isTurningSlectorsFinishSelection[i] && false)
            {
                allIsSelected = false;
                break;
            }
        }

        canLoadNextScene = allIsSelected && isTurningSlectorsFinishSelection[0];
        if(canLoadNextScene && !nextSceneIsLoading)
        {
            nextSceneIsLoading = true;
            LoadSelectoinMapScene();
        }

        for (int i = 0; i < indexToInit; i++)
        {
            if (isHelpCanvasOpen[i])
                continue;

            escapeButton.controllerType = controllerIndexs[i];
            if (escapeButton.IsPressedDown())
            {
                TransitionManager.instance.LoadSceneAsync("Screen Title", null);
            }
        }

        for (int i = 0; i < indexToInit; i++)
        {
            if (isHelpCanvasOpen[i])
                continue;

            helpButton.controllerType = controllerIndexs[i];
            if (helpButton.IsPressedDown())
            {
                OpenHelpCanvas(i);
            }
        }

        //DebugText.instance.text += turningSelectors[0].selectedItem.GetComponent<CharSelectorItemData>().gameObject.name;
    }

    private void OpenHelpCanvas(int indexTuringSelector)
    {
        isHelpCanvasOpen[indexTuringSelector] = true;
        GameObject selectedChar = turningSelectors[indexTuringSelector].selectedItem;
        int charNumber = selectedChar.GetComponent<CharSelectorItemData>().charNumber;
        helpCanvas[indexTuringSelector] = Instantiate(charHelperCanvasPrefabs[charNumber], transform);
        CharHelpCanvas charHelpCanvas = helpCanvas[indexTuringSelector].GetComponent<CharHelpCanvas>();
        charHelpCanvas.Lauch(turningSelectors[indexTuringSelector], controllerIndexs[indexTuringSelector], indexTuringSelector, OnCloseHelpCanvas);
    }

    private void OnCloseHelpCanvas(int indexHelpCanvas)
    {
        isHelpCanvasOpen[indexHelpCanvas] = false;
    }

    private void LoadSelectoinMapScene()
    {
        object[] data = new object[indexToInit];
        for (int i = 0; i < data.Length; i++)
        {
            CharSelectorItemData tmp = turningSelectors[i].selectedItem.GetComponent<CharSelectorItemData>();
            tmp.playerIndex = (PlayerIndex)(i + 1);
            tmp.controllerType = controllerIndexs[i];
            data[i] = tmp;
        }
        TransitionManager.instance.LoadSceneAsync("Selection Map", data);
    }

    private void RemoveSettingsIndex(int index)
    {
        if (index >= turningSelectors.Length)
            return;
        if(index == 3)
        {
            isTurningSelectorInit[index] = isTurningSlectorsFinishSelection[index] = false;
            controllerIndexs[indexToInit] = ControllerType.Keyboard;
            indexToInit--;
            return;
        }
        for (int i = index; i < turningSelectors.Length - 1; i++)
        {
            isTurningSelectorInit[i] = isTurningSelectorInit[i + 1];
            controllerIndexs[i] = controllerIndexs[i + 1];
            isTurningSlectorsFinishSelection[i] = isTurningSlectorsFinishSelection[i + 1];
        }
        isTurningSelectorInit[turningSelectors.Length - 1] = isTurningSlectorsFinishSelection[turningSelectors.Length - 1] = false;
        controllerIndexs[turningSelectors.Length - 1] = ControllerType.Keyboard;
        indexToInit = Mathf.Max(0, indexToInit - 1);
    }

    #region IsApplyingSelection / IsUnapplyingSelection / PressingUpOrDown

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

    private bool IsUnApplyingSelection(in ControllerType controllerType)
    {
        switch (controllerType)
        {
            case ControllerType.Keyboard:
                return InputManager.GetKeyDown(KeyCode.Escape) || InputManager.GetKeyDown(KeyCode.Backspace);
            case ControllerType.Gamepad1:
                return InputManager.GetKeyDown(KeyCode.Joystick1Button1);
            case ControllerType.Gamepad2:
                return InputManager.GetKeyDown(KeyCode.Joystick2Button1);
            case ControllerType.Gamepad3:
                return InputManager.GetKeyDown(KeyCode.Joystick3Button1);
            case ControllerType.Gamepad4:
                return InputManager.GetKeyDown(KeyCode.Joystick4Button1);
            case ControllerType.GamepadAll:
                return InputManager.GetKeyDown(KeyCode.Joystick1Button1) || InputManager.GetKeyDown(KeyCode.Joystick2Button1) ||
                    InputManager.GetKeyDown(KeyCode.Joystick3Button1) || InputManager.GetKeyDown(KeyCode.Joystick4Button1);
            case ControllerType.All:
                return InputManager.GetKeyDown(KeyCode.Escape) || InputManager.GetKeyDown(KeyCode.Backspace) ||
                    InputManager.GetKeyDown(KeyCode.Joystick1Button1) || InputManager.GetKeyDown(KeyCode.Joystick2Button1) ||
                    InputManager.GetKeyDown(KeyCode.Joystick3Button1) || InputManager.GetKeyDown(KeyCode.Joystick4Button1);
            default:
                return false;
        }
    }

    private bool IsPressingUpOrDown(in ControllerType controllerType, out bool up)
    {
        bool TestControllerType(in InputKey up, in InputKey down, in ControllerType controllerType, out bool b)
        {
            if (InputManager.GetKeyDown(up) || InputManager.GetGamepadStickUp(controllerType, GamepadStick.right) || InputManager.GetGamepadStickUp(controllerType, GamepadStick.left))
            {
                b = true;
                return true;
            }
            if (InputManager.GetKeyDown(down) || InputManager.GetGamepadStickDown(controllerType, GamepadStick.right) || InputManager.GetGamepadStickDown(controllerType, GamepadStick.left))
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
                if (InputManager.GetKeyDown(KeyCode.W) || InputManager.GetKeyDown(KeyCode.UpArrow))
                {
                    up = true;
                    return true;
                }
                if (InputManager.GetKeyDown(KeyCode.S) || InputManager.GetKeyDown(KeyCode.DownArrow))
                {
                    up = false;
                    return true;
                }
                up = false;
                return false;
            case ControllerType.Gamepad1:
                return TestControllerType(InputKey.GP1DPadUp, InputKey.GP1DPadDown, ControllerType.Gamepad1, out up);
            case ControllerType.Gamepad2:
                return TestControllerType(InputKey.GP2DPadUp, InputKey.GP2DPadDown, ControllerType.Gamepad2, out up);
            case ControllerType.Gamepad3:
                return TestControllerType(InputKey.GP3DPadUp, InputKey.GP3DPadDown, ControllerType.Gamepad3, out up);
            case ControllerType.Gamepad4:
                return TestControllerType(InputKey.GP4DPadUp, InputKey.GP4DPadDown, ControllerType.Gamepad4, out up);
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

    #endregion

    #region ControllerAlreadyInit / NewCOntrollerPress a key

    private bool ControllerIsAlreadyInit(ControllerType controllerType, out int index)
    {
        for (int i = 0; i < 4; i++)
        {
            if (isTurningSelectorInit[i] && controllerIndexs[i] == controllerType)
            {
                index = i;
                return true;
            }
        }
        index = 0;
        return false;
    }

    private bool NewControllerIsPressingAKey(out ControllerType controllerType, out InputKey key)
    {
        bool TestControllerType(in ControllerType controllerType, out InputKey key)
        {
            if (!ControllerIsAlreadyInit(controllerType, out int i) && 
                (controllerType == ControllerType.Keyboard || InputManager.IsGamePadConnected(controllerType)))
            {
                if (InputManager.Listen(controllerType, out key))
                    return true;
            }
            key = (int)KeyCode.None;
            return false;
        }
        
        if(TestControllerType(ControllerType.Keyboard, out key))
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