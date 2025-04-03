using TMPro;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class ControlManagerSettingMenu : MonoBehaviour
{
    [SerializeField] private SelectableUI masterVolume;
    [SerializeField] private SelectableUI musicVolume;
    [SerializeField] private SelectableUI soundFXVolume;
    [SerializeField] private SelectableUI windowMode;
    [SerializeField] private SelectableUI resolution;
    [SerializeField] private SelectableUI framerate;
    [SerializeField] private SelectableUI vsync;
    [SerializeField] private SelectableUI language;
    [SerializeField] private SelectableUI defaultButton;
    [SerializeField] private SelectableUI applyButton;

    [Space, Space]
    [SerializeField] private TextMeshProUGUI controlText;
    [SerializeField] private DropDownSelectableUI inputTypeDropdown;
    [SerializeField] private ControlItem moveUp;
    [SerializeField] private ControlItem moveDown;
    [SerializeField] private ControlItem moveRight;
    [SerializeField] private ControlItem moveLeft;
    [SerializeField] private ControlItem dashControl;
    [SerializeField] private ControlItem jumpControl;
    [SerializeField] private ControlItem attack1Control;
    [SerializeField] private ControlItem attack2Control;
    [SerializeField] private ControlItem grapControl;
    [SerializeField] private ControlItem interactControl;

    [HideInInspector] public SettingMenu settingMenu;
    public bool isAnInputListening => ControlItem.isAnInputListening;

    public BaseController GetSelectedBaseController() => inputTypeDropdown.value == 0 ? BaseController.Keyboard : BaseController.Gamepad;

    private void RefreshSettings()
    {
        controlText.text = LanguageManager.instance.GetText("controlTextSettingMenu").Resolve();
        moveUp.SetNameText(LanguageManager.instance.GetText("moveUpTextSettingMenu").Resolve());
        moveDown.SetNameText(LanguageManager.instance.GetText("moveDownTextSettingMenu").Resolve());
        moveRight.SetNameText(LanguageManager.instance.GetText("moveRightTextSettingMenu").Resolve());
        moveLeft.SetNameText(LanguageManager.instance.GetText("moveLeftTextSettingMenu").Resolve());
        dashControl.SetNameText(LanguageManager.instance.GetText("dashTextSettingMenu").Resolve());
        jumpControl.SetNameText(LanguageManager.instance.GetText("jumpTextSettingMenu").Resolve());
        attack1Control.SetNameText(LanguageManager.instance.GetText("attack1TextSettingMenu").Resolve());
        attack2Control.SetNameText(LanguageManager.instance.GetText("attack2TextSettingMenu").Resolve());
        grapControl.SetNameText(LanguageManager.instance.GetText("grabTextSettingMenu").Resolve());
        interactControl.SetNameText(LanguageManager.instance.GetText("interactTextSettingMenu").Resolve());
    }

    private void RefreshControl(bool defaultControl)
    {
        inputTypeDropdown.options = new List<TMP_Dropdown.OptionData>()
        {
            new TMP_Dropdown.OptionData() { text = LanguageManager.instance.GetText("keyboard").Resolve() },
            new TMP_Dropdown.OptionData() { text = LanguageManager.instance.GetText("gamepad").Resolve() }
        };

        inputTypeDropdown.onValueChanged.RemoveAllListeners();
        inputTypeDropdown.onValueChanged.AddListener(OnInputTypeChanged);

        SetKeysKey(defaultControl);
        SetUINeighbourhood();
    }

    private void OnInputTypeChanged(int value)
    {
        BaseController curCon = GetSelectedBaseController();
        bool activeKB = curCon == BaseController.Keyboard;
        moveUp.transform.parent.gameObject.SetActive(activeKB);
        moveDown.transform.parent.gameObject.SetActive(activeKB);
        moveRight.transform.parent.gameObject.SetActive(activeKB);
        moveLeft.transform.parent.gameObject.SetActive(activeKB);
        SetKeysKey(false);
        SetUINeighbourhood();
    }

    public void OnApplyButtonDown()
    {
        BaseController curCon = GetSelectedBaseController();

        if (curCon == BaseController.Keyboard)
        {
            InputManager.ReplaceAction("MoveUp", moveUp.key, curCon);
            InputManager.ReplaceAction("MoveDown", moveDown.key, curCon);
            InputManager.ReplaceAction("MoveRight", moveRight.key, curCon);
            InputManager.ReplaceAction("MoveLeft", moveLeft.key, curCon);
        }

        InputManager.ReplaceAction("Dash", dashControl.key, curCon);
        InputManager.ReplaceAction("Jump", jumpControl.key, curCon);
        InputManager.ReplaceAction("AttackWeak", attack1Control.key, curCon);
        InputManager.ReplaceAction("AttackStrong", attack2Control.key, curCon);
        InputManager.ReplaceAction("Grab", grapControl.key, curCon);
        InputManager.ReplaceAction("Interact", interactControl.key, curCon);
        InputEditor.instance.SaveInputConfig();

        RefreshSettings();
        RefreshControl(false);
    }

    public bool IsSomeControlUnApply()
    {
        bool res = !(InputManager.GetInputKey("Dash", BaseController.Keyboard, false)[0] == dashControl.keyboardKey &&
                InputManager.GetInputKey("Jump", BaseController.Keyboard, false)[0] == jumpControl.keyboardKey &&
                InputManager.GetInputKey("AttackWeak", BaseController.Keyboard, false)[0] == attack1Control.keyboardKey &&
                InputManager.GetInputKey("AttackStrong", BaseController.Keyboard, false)[0] == attack2Control.keyboardKey &&
                InputManager.GetInputKey("Grab", BaseController.Keyboard, false)[0] == grapControl.keyboardKey &&
                InputManager.GetInputKey("Interact", BaseController.Keyboard, false)[0] == interactControl.keyboardKey &&
                InputManager.GetInputKey("MoveUp", BaseController.Keyboard, false)[0] == moveUp.keyboardKey &&
                InputManager.GetInputKey("MoveDown", BaseController.Keyboard, false)[0] == moveDown.keyboardKey &&
                InputManager.GetInputKey("MoveRight", BaseController.Keyboard, false)[0] == moveRight.keyboardKey &&
                InputManager.GetInputKey("MoveLeft", BaseController.Keyboard, false)[0] == moveLeft.keyboardKey);

        res = res || !(InputManager.GetInputKey("Dash", BaseController.Gamepad, false)[0] == dashControl.gamepadKey &&
                InputManager.GetInputKey("Jump", BaseController.Gamepad, false)[0] == jumpControl.gamepadKey &&
                InputManager.GetInputKey("AttackWeak", BaseController.Gamepad, false)[0] == attack1Control.gamepadKey &&
                InputManager.GetInputKey("AttackStrong", BaseController.Gamepad, false)[0] == attack2Control.gamepadKey &&
                InputManager.GetInputKey("Grab", BaseController.Gamepad, false)[0] == grapControl.gamepadKey &&
                InputManager.GetInputKey("Interact", BaseController.Gamepad, false)[0] == interactControl.gamepadKey);
        return res;
    }

    public void OnDefaultButtonDown()
    {
        RefreshSettings();
        RefreshControl(true);
    }

    private void SetKeysKey(bool defaultConfig)
    {
        BaseController curCon = GetSelectedBaseController();
        if (curCon == BaseController.Keyboard)
        {
            moveUp.SetCurrentKey(InputManager.GetInputKey("MoveUp", curCon, defaultConfig)[0], curCon);
            moveDown.SetCurrentKey(InputManager.GetInputKey("MoveDown", curCon, defaultConfig)[0], curCon);
            moveRight.SetCurrentKey(InputManager.GetInputKey("MoveRight", curCon, defaultConfig)[0], curCon);
            moveLeft.SetCurrentKey(InputManager.GetInputKey("MoveLeft", curCon, defaultConfig)[0], curCon);
        }

        dashControl.SetCurrentKey(InputManager.GetInputKey("Dash", curCon, defaultConfig)[0], curCon);
        jumpControl.SetCurrentKey(InputManager.GetInputKey("Jump", curCon, defaultConfig)[0], curCon);
        attack1Control.SetCurrentKey(InputManager.GetInputKey("AttackWeak", curCon, defaultConfig)[0], curCon);
        attack2Control.SetCurrentKey(InputManager.GetInputKey("AttackStrong", curCon, defaultConfig)[0], curCon);
        grapControl.SetCurrentKey(InputManager.GetInputKey("Grab", curCon, defaultConfig)[0], curCon);
        interactControl.SetCurrentKey(InputManager.GetInputKey("Interact", curCon, defaultConfig)[0], curCon);
    }

    private void EnableUIElementsInternal(bool enable)
    {
        if (GetSelectedBaseController() == BaseController.Keyboard)
        {
            moveUp.interactable = enable;
            moveDown.interactable = enable;
            moveRight.interactable = enable;
            moveLeft.interactable = enable;
        }
        dashControl.interactable = enable;
        jumpControl.interactable = enable;
        attack1Control.interactable = enable;
        attack2Control.interactable = enable;
        grapControl.interactable = enable;
        interactControl.interactable = enable;
        inputTypeDropdown.interactable = enable;
    }

    public void DisableUIElements()
    {
        EnableUIElementsInternal(false);
    }

    public void EnableUIElements()
    {
        EnableUIElementsInternal(true);
    }

    private void SetUINeighbourhood()
    {
        bool kbLayout = GetSelectedBaseController() == BaseController.Keyboard;
        if(kbLayout)
        {
            inputTypeDropdown.upSelectableUI = interactControl.selectableUI;
            inputTypeDropdown.downSelectableUI = moveUp.selectableUI;
            inputTypeDropdown.rightSelectableUI = masterVolume;
            inputTypeDropdown.leftSelectableUI = masterVolume;

            moveUp.selectableUI.upSelectableUI = inputTypeDropdown;
            moveUp.selectableUI.downSelectableUI = moveDown.selectableUI;
            moveUp.selectableUI.rightSelectableUI = musicVolume;
            moveUp.selectableUI.leftSelectableUI = musicVolume;

            moveDown.selectableUI.upSelectableUI = moveUp.selectableUI;
            moveDown.selectableUI.downSelectableUI = moveRight.selectableUI;
            moveDown.selectableUI.rightSelectableUI = soundFXVolume;
            moveDown.selectableUI.leftSelectableUI = soundFXVolume;

            moveRight.selectableUI.upSelectableUI = moveDown.selectableUI;
            moveRight.selectableUI.downSelectableUI = moveLeft.selectableUI;
            moveRight.selectableUI.rightSelectableUI = windowMode;
            moveRight.selectableUI.leftSelectableUI = windowMode;

            moveLeft.selectableUI.upSelectableUI = moveRight.selectableUI;
            moveLeft.selectableUI.downSelectableUI = dashControl.selectableUI;
            moveLeft.selectableUI.rightSelectableUI = resolution;
            moveLeft.selectableUI.leftSelectableUI = resolution;

            dashControl.selectableUI.upSelectableUI = moveLeft.selectableUI;
            dashControl.selectableUI.downSelectableUI = jumpControl.selectableUI;
            dashControl.selectableUI.rightSelectableUI = framerate;
            dashControl.selectableUI.leftSelectableUI = framerate;

            jumpControl.selectableUI.upSelectableUI = dashControl.selectableUI;
            jumpControl.selectableUI.downSelectableUI = grapControl.selectableUI;
            jumpControl.selectableUI.rightSelectableUI = vsync;
            jumpControl.selectableUI.leftSelectableUI = vsync;

            grapControl.selectableUI.upSelectableUI = jumpControl.selectableUI;
            grapControl.selectableUI.downSelectableUI = attack1Control.selectableUI;
            grapControl.selectableUI.rightSelectableUI = vsync;
            grapControl.selectableUI.leftSelectableUI = vsync;

            attack1Control.selectableUI.upSelectableUI = grapControl.selectableUI;
            attack1Control.selectableUI.downSelectableUI = attack2Control.selectableUI;
            attack1Control.selectableUI.rightSelectableUI = language;
            attack1Control.selectableUI.leftSelectableUI = language;

            attack2Control.selectableUI.upSelectableUI = attack1Control.selectableUI;
            attack2Control.selectableUI.downSelectableUI = interactControl.selectableUI;
            attack2Control.selectableUI.rightSelectableUI = language;
            attack2Control.selectableUI.leftSelectableUI = language;

            interactControl.selectableUI.upSelectableUI = attack2Control.selectableUI;
            interactControl.selectableUI.downSelectableUI = inputTypeDropdown;
            interactControl.selectableUI.rightSelectableUI = applyButton;
            interactControl.selectableUI.leftSelectableUI = applyButton;
        }
        else
        {
            inputTypeDropdown.upSelectableUI = interactControl.selectableUI;
            inputTypeDropdown.downSelectableUI = dashControl.selectableUI;
            inputTypeDropdown.rightSelectableUI = masterVolume;
            inputTypeDropdown.leftSelectableUI = masterVolume;

            dashControl.selectableUI.upSelectableUI = inputTypeDropdown;
            dashControl.selectableUI.downSelectableUI = jumpControl.selectableUI;
            dashControl.selectableUI.rightSelectableUI = musicVolume;
            dashControl.selectableUI.leftSelectableUI = musicVolume;

            jumpControl.selectableUI.upSelectableUI = dashControl.selectableUI;
            jumpControl.selectableUI.downSelectableUI = grapControl.selectableUI;
            jumpControl.selectableUI.rightSelectableUI = soundFXVolume;
            jumpControl.selectableUI.leftSelectableUI = soundFXVolume;

            grapControl.selectableUI.upSelectableUI = jumpControl.selectableUI;
            grapControl.selectableUI.downSelectableUI = attack1Control.selectableUI;
            grapControl.selectableUI.rightSelectableUI = windowMode;
            grapControl.selectableUI.leftSelectableUI = windowMode;

            attack1Control.selectableUI.upSelectableUI = grapControl.selectableUI;
            attack1Control.selectableUI.downSelectableUI = attack2Control.selectableUI;
            attack1Control.selectableUI.rightSelectableUI = resolution;
            attack1Control.selectableUI.leftSelectableUI = resolution;

            attack2Control.selectableUI.upSelectableUI = attack1Control.selectableUI;
            attack2Control.selectableUI.downSelectableUI = interactControl.selectableUI;
            attack2Control.selectableUI.rightSelectableUI = framerate;
            attack2Control.selectableUI.leftSelectableUI = framerate;

            interactControl.selectableUI.upSelectableUI = attack2Control.selectableUI;
            interactControl.selectableUI.downSelectableUI = inputTypeDropdown;
            interactControl.selectableUI.rightSelectableUI = vsync;
            interactControl.selectableUI.leftSelectableUI = vsync;
        }
    }


    private void OnControlItemListening(ControlItem controlItem, bool start)
    {
        List<ControlItem> controlItems = new List<ControlItem>(10)
        {
            dashControl, jumpControl, attack1Control, attack2Control, grapControl, interactControl
        };

        if (GetSelectedBaseController() == BaseController.Keyboard)
        {
            controlItems.Add(moveUp);
            controlItems.Add(moveDown);
            controlItems.Add(moveRight);
            controlItems.Add(moveLeft);
        }

        for (int i = 0; i < controlItems.Count; i++)
        {
            if (controlItems[i] != controlItem)
            {
                controlItems[i].interactable = !start;
            }
        }

        inputTypeDropdown.interactable = !start;
        if(start)
            settingMenu.OnControlItemStartListening(controlItem);
        else
            settingMenu.OnControlItemStopListening(controlItem);
    }

    public void OnControlItemStartListening(ControlItem controlItem)
    {
        OnControlItemListening(controlItem, true);
    }

    public void OnControlItemStopListening(ControlItem controlItem)
    {
        OnControlItemListening(controlItem, false);
    }

    private void OnEnable()
    {
        StartCoroutine(OnEnableCorout());
    }

    private IEnumerator OnEnableCorout()
    {
        yield return null;
        yield return null;

        moveUp.controlManagerSettingMenu = this;
        moveUp.Init(InputManager.GetInputKey("MoveUp", BaseController.Keyboard)[0], InputManager.GetInputKey("MoveUp", BaseController.Gamepad)[0]);
        moveDown.controlManagerSettingMenu = this;
        moveDown.Init(InputManager.GetInputKey("MoveDown", BaseController.Keyboard)[0], InputManager.GetInputKey("MoveDown", BaseController.Gamepad)[0]);
        moveLeft.controlManagerSettingMenu = this;
        moveLeft.Init(InputManager.GetInputKey("MoveLeft", BaseController.Keyboard)[0], InputManager.GetInputKey("MoveLeft", BaseController.Gamepad)[0]);
        moveRight.controlManagerSettingMenu = this;
        moveRight.Init(InputManager.GetInputKey("MoveRight", BaseController.Keyboard)[0], InputManager.GetInputKey("MoveRight", BaseController.Gamepad)[0]);
        dashControl.controlManagerSettingMenu = this;
        dashControl.Init(InputManager.GetInputKey("Dash", BaseController.Keyboard)[0], InputManager.GetInputKey("Dash", BaseController.Gamepad)[0]);
        jumpControl.controlManagerSettingMenu = this;
        jumpControl.Init(InputManager.GetInputKey("Jump", BaseController.Keyboard)[0], InputManager.GetInputKey("Jump", BaseController.Gamepad)[0]);
        attack1Control.controlManagerSettingMenu = this;
        attack1Control.Init(InputManager.GetInputKey("MovAttackWeakeUp", BaseController.Keyboard)[0], InputManager.GetInputKey("AttackWeak", BaseController.Gamepad)[0]);
        attack2Control.controlManagerSettingMenu = this;
        attack2Control.Init(InputManager.GetInputKey("AttackStrong", BaseController.Keyboard)[0], InputManager.GetInputKey("AttackStrong", BaseController.Gamepad)[0]);
        grapControl.controlManagerSettingMenu = this;
        grapControl.Init(InputManager.GetInputKey("Grab", BaseController.Keyboard)[0], InputManager.GetInputKey("Grab", BaseController.Gamepad)[0]);
        interactControl.controlManagerSettingMenu = this;
        interactControl.Init(InputManager.GetInputKey("Interact", BaseController.Keyboard)[0], InputManager.GetInputKey("Interact", BaseController.Gamepad)[0]);

        inputTypeDropdown.value = 0;

        RefreshSettings();
        RefreshControl(false);
    }
}
