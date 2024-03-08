using TMPro;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class ControlManagerSettingMenu : MonoBehaviour
{
    public static ControlManagerSettingMenu instance;

    private ControlItem listeningInput;
    public bool isInputListening => listeningInput != null;

    [SerializeField] private TextMeshProUGUI controlText;
    [SerializeField] private TMP_Dropdown inputTypeDropdown;
    [SerializeField] private ControlItem moveUp;
    [SerializeField] private ControlItem moveDown;
    [SerializeField] private ControlItem moveRight;
    [SerializeField] private ControlItem moveLeft;
    [SerializeField] private ControlItem dashControl;
    [SerializeField] private ControlItem jumpControl;
    [SerializeField] private ControlItem attack1Control;
    [SerializeField] private ControlItem attack2Control;
    [SerializeField] private ControlItem grapControl;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

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
    }

    private void OnInputTypeChanged(int value)
    {
        if(listeningInput != null)
        {
            listeningInput.StopListening();
            listeningInput = null;
        }

        BaseController curCon = GetSelectedBaseController();
        bool activeKB = curCon == BaseController.Keyboard;
        moveUp.gameObject.SetActive(activeKB);
        moveDown.gameObject.SetActive(activeKB);
        moveRight.gameObject.SetActive(activeKB);
        moveLeft.gameObject.SetActive(activeKB);
        SetKeysKey(false);
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
        InputEditor.instance.SaveInputConfig();
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
    }

    private void OnEnable()
    {
        StartCoroutine(OnEnableCorout());
    }

    private IEnumerator OnEnableCorout()
    {
        yield return null;
        yield return null;

        inputTypeDropdown.value = 0;

        RefreshSettings();
        RefreshControl(false);
    }
}
