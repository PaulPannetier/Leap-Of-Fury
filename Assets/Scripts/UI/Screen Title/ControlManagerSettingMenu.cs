using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class ControlManagerSettingMenu : MonoBehaviour
{
    private ControllerType currentControllerType => inputTypeDropdown.value == 0 ? ControllerType.Keyboard : ControllerType.GamepadAll;

    [SerializeField] private TextMeshProUGUI controlText;
    [SerializeField] private TMP_Dropdown inputTypeDropdown;
    [SerializeField] private ControlItem dashControl;
    [SerializeField] private ControlItem jumpControl;
    [SerializeField] private ControlItem attack1Control;
    [SerializeField] private ControlItem attack2Control;
    [SerializeField] private ControlItem grapControl;

    private void Start()
    {
        controlText.text = LanguageManager.instance.GetText("controlTextSettingMenu");
        dashControl.SetNameText(LanguageManager.instance.GetText("dashTextSettingMenu"));
        jumpControl.SetNameText(LanguageManager.instance.GetText("jumpTextSettingMenu"));
        attack1Control.SetNameText(LanguageManager.instance.GetText("attack1TextSettingMenu"));
        attack2Control.SetNameText(LanguageManager.instance.GetText("attack2TextSettingMenu"));
        grapControl.SetNameText(LanguageManager.instance.GetText("grabTextSettingMenu"));

        inputTypeDropdown.options = new List<TMP_Dropdown.OptionData>()
        {
            new TMP_Dropdown.OptionData() { text = LanguageManager.instance.GetText("keyboard") },
            new TMP_Dropdown.OptionData() { text = LanguageManager.instance.GetText("gamepad") }
        };
        inputTypeDropdown.value = 0;

        SetKeyText();
    }

    private void SetKeyText()
    {
        ControllerType curCon = currentControllerType;
        //dashControl.SetKeyText(InputManager.KeyToString("Dash", ))
    }
}
