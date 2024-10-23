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

    public BaseController GetSelectedBaseController() => inputTypeDropdown.value == 0 ? BaseController.Keyboard : BaseController.Gamepad;

    private void RefreshSettings()
    {
        controlText.text = LanguageManager.instance.GetText("controlTextSettingMenu");
        moveUp.SetNameText(LanguageManager.instance.GetText("moveUpTextSettingMenu"));
        moveDown.SetNameText(LanguageManager.instance.GetText("moveDownTextSettingMenu"));
        moveRight.SetNameText(LanguageManager.instance.GetText("moveRightTextSettingMenu"));
        moveLeft.SetNameText(LanguageManager.instance.GetText("moveLeftTextSettingMenu"));
        dashControl.SetNameText(LanguageManager.instance.GetText("dashTextSettingMenu"));
        jumpControl.SetNameText(LanguageManager.instance.GetText("jumpTextSettingMenu"));
        attack1Control.SetNameText(LanguageManager.instance.GetText("attack1TextSettingMenu"));
        attack2Control.SetNameText(LanguageManager.instance.GetText("attack2TextSettingMenu"));
        grapControl.SetNameText(LanguageManager.instance.GetText("grabTextSettingMenu"));
        interactControl.SetNameText(LanguageManager.instance.GetText("interactTextSettingMenu"));
    }

    private void RefreshControl(bool defaultControl)
    {
        inputTypeDropdown.options = new List<TMP_Dropdown.OptionData>()
        {
            new TMP_Dropdown.OptionData() { text = LanguageManager.instance.GetText("keyboard") },
            new TMP_Dropdown.OptionData() { text = LanguageManager.instance.GetText("gamepad") }
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
        moveUp.gameObject.SetActive(activeKB);
        moveDown.gameObject.SetActive(activeKB);
        moveRight.gameObject.SetActive(activeKB);
        moveLeft.gameObject.SetActive(activeKB);
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
        bool IsSomeControlUnApplyBaseController(BaseController controller)
        {
            bool common = InputManager.GetInputKey("Dash", controller, false).Contains(dashControl.key) &&
                InputManager.GetInputKey("Jump", controller, false).Contains(jumpControl.key) &&
                InputManager.GetInputKey("AttackWeak", controller, false).Contains(attack1Control.key) &&
                InputManager.GetInputKey("AttackStrong", controller, false).Contains(attack2Control.key) &&
                InputManager.GetInputKey("Grab", controller, false).Contains(grapControl.key) &&
                InputManager.GetInputKey("Interact", controller, false).Contains(interactControl.key);
            if(controller == BaseController.Gamepad)
                return !common;

            return !(common && InputManager.GetInputKey("MoveUp", controller, false).Contains(moveUp.key) &&
                InputManager.GetInputKey("MoveDown", controller, false).Contains(moveDown.key) &&
                InputManager.GetInputKey("MoveRight", controller, false).Contains(moveRight.key) &&
                InputManager.GetInputKey("MoveLeft", controller, false).Contains(moveLeft.key));
        }
        bool a = IsSomeControlUnApplyBaseController(BaseController.Keyboard);
        bool b = IsSomeControlUnApplyBaseController(BaseController.Gamepad);
        return a || b;
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
            moveUp.key = InputManager.GetInputKey("MoveUp", curCon, defaultConfig)[0];
            moveDown.key = InputManager.GetInputKey("MoveDown", curCon, defaultConfig)[0];
            moveRight.key = InputManager.GetInputKey("MoveRight", curCon, defaultConfig)[0];
            moveLeft.key = InputManager.GetInputKey("MoveLeft", curCon, defaultConfig)[0];
        }

        dashControl.key = InputManager.GetInputKey("Dash", curCon, defaultConfig)[0];
        jumpControl.key = InputManager.GetInputKey("Jump", curCon, defaultConfig)[0];
        attack1Control.key = InputManager.GetInputKey("AttackWeak", curCon, defaultConfig)[0];
        attack2Control.key = InputManager.GetInputKey("AttackStrong", curCon, defaultConfig)[0];
        grapControl.key = InputManager.GetInputKey("Grab", curCon, defaultConfig)[0];
        interactControl.key = InputManager.GetInputKey("Interact", curCon, defaultConfig)[0];
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

    private void OnEnable()
    {
        StartCoroutine(OnEnableCorout());
    }

    private IEnumerator OnEnableCorout()
    {
        yield return null;
        yield return null;

        moveUp.controlManagerSettingMenu = this;
        moveDown.controlManagerSettingMenu = this;
        moveLeft.controlManagerSettingMenu = this;
        moveRight.controlManagerSettingMenu = this;
        dashControl.controlManagerSettingMenu = this;
        jumpControl.controlManagerSettingMenu = this;
        attack1Control.controlManagerSettingMenu = this;
        attack2Control.controlManagerSettingMenu = this;
        grapControl.controlManagerSettingMenu = this;
        interactControl.controlManagerSettingMenu = this;

        inputTypeDropdown.value = 0;

        RefreshSettings();
        RefreshControl(false);
    }
}
