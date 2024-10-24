using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SettingMenu : MonoBehaviour
{
    private static readonly Dictionary<int, FullScreenMode> convertIntToFullScreenMode = new Dictionary<int, FullScreenMode>
    {
        { 0, FullScreenMode.FullScreenWindow },
        { 1, FullScreenMode.Windowed }
    };

    private static readonly Dictionary<FullScreenMode, int> convertFullScreenModeToInt = new Dictionary<FullScreenMode, int>
    {
        { FullScreenMode.FullScreenWindow, 0 },
        { FullScreenMode.Windowed, 1 }
    };

    private Vector2Int[] availableResolutions;
    private RefreshRate[] availableFramerate;
    private bool isEnable;

    [SerializeField] private SliderSelectableUI masterSlider;
    [SerializeField] private TMP_Text masterText;
    [SerializeField] private SliderSelectableUI musicSlider;
    [SerializeField] private TMP_Text musicText;
    [SerializeField] private SliderSelectableUI soundFXSlider;
    [SerializeField] private TMP_Text soundFXText;
    [SerializeField] private DropDownSelectableUI windowModeDropdown;
    [SerializeField] private TMP_Text windowModeText;
    [SerializeField] private DropDownSelectableUI resolutionDropdown;
    [SerializeField] private TMP_Text resolutionText;
    [SerializeField] private DropDownSelectableUI framerateDropdown;
    [SerializeField] private TMP_Text framerateText;
    [SerializeField] private CheckboxSelectableUI vSynchToggle;
    [SerializeField] private TMP_Text vSynchText;
    [SerializeField] private DropDownSelectableUI languageDropdown;
    [SerializeField] private TMP_Text languageText;
    [SerializeField] private ButtonSelectableUI applyButton;
    [SerializeField] private TMP_Text applyButtonText;
    [SerializeField] private ButtonSelectableUI defaultButton;
    [SerializeField] private TMP_Text defaultButtonText;
    [SerializeField] private InputManager.GeneralInput echapInput;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject warningNoApplyPanel;
    [SerializeField] private SelectableUIGroup groupMenu;
    [SerializeField] private ControlManagerSettingMenu controlManagerSettingMenu;

    public bool isUIElementActive => groupMenu.selectedUI != null && (groupMenu.selectedUI.isActive || groupMenu.selectedUI.isDesactivatedThisFrame);

    private void Awake()
    {
        isEnable = false;
    }

    private void Start()
    {
        applyButton.onButtonPressed.RemoveAllListeners();
        applyButton.onButtonPressed.AddListener(OnApplyButtonDown);
        defaultButton.onButtonPressed.RemoveAllListeners();
        defaultButton.onButtonPressed.AddListener(OnDefaultButtonDown);
        controlManagerSettingMenu.settingMenu = this;
    }

    private void InitOptions()
    {
        //Window dropdown
        List<TMP_Dropdown.OptionData> windowModeOptions = new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData(LanguageManager.instance.GetText("fullscreenMode"), windowModeDropdown.options[0].image),
            new TMP_Dropdown.OptionData(LanguageManager.instance.GetText("windowedMode"), windowModeDropdown.options[0].image),
        };
        windowModeDropdown.options = windowModeOptions;

        //resolution dropdown
        Sprite resolutionSprite = resolutionDropdown.options[0].image;
        List<TMP_Dropdown.OptionData> resolutionOptions = new List<TMP_Dropdown.OptionData>();
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            resolutionOptions.Add(new TMP_Dropdown.OptionData(Vector2IntToResolution(availableResolutions[i]), resolutionSprite));
        }

        string Vector2IntToResolution(in Vector2Int v)
        {
            return v.x.ToString() + " x " + v.y.ToString();
        }

        resolutionDropdown.options = resolutionOptions;

        //framerate dropdown
        List<TMP_Dropdown.OptionData> framerateOptions = new List<TMP_Dropdown.OptionData>();
        Sprite framerateSprite = framerateDropdown.options[0].image;
        for (int i = 0; i < availableFramerate.Length; i++)
        {
            framerateOptions.Add(new TMP_Dropdown.OptionData(availableFramerate[i].value.Round().ToString() + " Hz", framerateSprite));
        }
        framerateDropdown.options = framerateOptions;

        //language dropdown
        List<TMP_Dropdown.OptionData> languageOptions = new List<TMP_Dropdown.OptionData>();
        Sprite languageSprite = languageDropdown.options[0].image;
        foreach (string language in LanguageManager.instance.availableLanguage)
        {
            languageOptions.Add(new TMP_Dropdown.OptionData(language, languageSprite));
        }
        languageDropdown.options = languageOptions;
    }

    private void Update()
    {
        if (!isEnable)
            return;

        if ((echapInput.controllerType == ControllerType.Keyboard || !isUIElementActive) && echapInput.IsPressedDown())
        {
            CloseSettingsMenu();
            return;
        }
    }

    private bool IsSomeSettingUnapply()
    {
        SettingsManager.ConfigurationData settingsConfig = GetMenuConfiguration();
        SettingsManager.ConfigurationData currentConfig = SettingsManager.instance.currentConfig;
        bool differentConfig = currentConfig != settingsConfig;
        return differentConfig || controlManagerSettingMenu.IsSomeControlUnApply();
    }

    private void CloseSettingsMenu(bool force = false)
    {
        bool isDifferentSetting = IsSomeSettingUnapply();
        if (!force && isDifferentSetting)
        {
            warningNoApplyPanel.SetActive(true);
            DisableUISettingsElements();
        }

        if(force || !isDifferentSetting)
        {
            isEnable = false;
            groupMenu.ResetToDefault();
            mainMenu.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    private void EnableUISettingsElements(bool enable, bool controlSetting = true)
    {
        groupMenu.enableBehaviour = enable;
        masterSlider.interactable = enable;
        musicSlider.interactable = enable;
        soundFXSlider.interactable = enable;
        windowModeDropdown.interactable = enable;
        resolutionDropdown.interactable = enable;
        framerateDropdown.interactable = enable;
        vSynchToggle.interactable = enable;
        languageDropdown.interactable = enable;
        applyButton.interactable = enable;
        defaultButton.interactable = enable;
        if(controlSetting)
        {
            if (enable)
                controlManagerSettingMenu.EnableUIElements();
            else
                controlManagerSettingMenu.DisableUIElements();
        }
    }

    private void DisableUISettingsElements(bool controlSetting = true)
    {
        EnableUISettingsElements(false, controlSetting);
    }

    private void EnableUISettingsElements(bool controlSetting = true)
    {
        EnableUISettingsElements(true, controlSetting);
    }

    public void OnYesUnapplyButtonDown()
    {
        OnApplyButtonDown();
        CloseSettingsMenu();
    }

    public void OnNoUnapplyButtonDown()
    {
        CloseSettingsMenu(true);
    }

    public void OnControlItemStartListening(ControlItem controlItem)
    {
        EnableUISettingsElements(false, false);
    }

    public void OnControlItemStopListening(ControlItem controlItem)
    {
        EnableUISettingsElements(true, false);
    }

    private void RefreshText()
    {
        masterText.text = LanguageManager.instance.GetText("masterVolume") + " :";
        musicText.text = LanguageManager.instance.GetText("musicVolume") + " :";
        soundFXText.text = LanguageManager.instance.GetText("soundFXVolume") + " :";
        windowModeText.text = LanguageManager.instance.GetText("windowMode") + " :";
        resolutionText.text = LanguageManager.instance.GetText("resolution") + " :";
        framerateText.text = LanguageManager.instance.GetText("framerate") + " :";
        languageText.text = LanguageManager.instance.GetText("language") + " :";

        //VSynch
        vSynchText.text = LanguageManager.instance.GetText("vsync") + " :";

        //buttons
        applyButtonText.text = LanguageManager.instance.GetText("applyOptionButton");
        defaultButtonText.text = LanguageManager.instance.GetText("defaultOptionButton");

        List<TMP_Dropdown.OptionData> windowModeOptions = new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData(LanguageManager.instance.GetText("fullscreenMode"), windowModeDropdown.options[0].image),
            new TMP_Dropdown.OptionData(LanguageManager.instance.GetText("windowedMode"), windowModeDropdown.options[0].image),
        };
        windowModeDropdown.options = windowModeOptions;
    }

    private SettingsManager.ConfigurationData GetMenuConfiguration()
    {
        Vector2Int resolution = availableResolutions[resolutionDropdown.value];
        RefreshRate targetedFPS = availableFramerate[framerateDropdown.value];
        string language = LanguageManager.instance.availableLanguage[languageDropdown.value];
        FullScreenMode windowMode = convertIntToFullScreenMode[windowModeDropdown.value];
        return new SettingsManager.ConfigurationData(masterSlider.value, musicSlider.value, soundFXSlider.value, resolution, targetedFPS, language, windowMode, false, vSynchToggle.isOn);
    }

    public void OnApplyButtonDown()
    {
        SettingsManager.ConfigurationData configurationData = GetMenuConfiguration();

        SettingsManager.instance.SetCurrentConfig(configurationData);

        RefreshText();

        controlManagerSettingMenu.OnApplyButtonDown();
    }

    private void ShowConfig(in SettingsManager.ConfigurationData configurationData)
    {
        masterSlider.value = configurationData.masterVolume;
        musicSlider.value = configurationData.musicVolume;
        soundFXSlider.value = configurationData.soundFXVolume;

        windowModeDropdown.value = convertFullScreenModeToInt[configurationData.windowMode];

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            if (availableResolutions[i] == configurationData.resolusion)
            {
                resolutionDropdown.value = i;
                break;
            }
        }

        for (int i = 0; i < availableFramerate.Length; i++)
        {
            if (availableFramerate[i].value.Round() == configurationData.targetedFPS.value.Round())
            {
                framerateDropdown.value = i;
                break;
            }
        }

        vSynchToggle.isOn = configurationData.vSync;

        for (int i = 0; i < LanguageManager.instance.availableLanguage.Length; i++)
        {
            if (configurationData.language == LanguageManager.instance.availableLanguage[i])
            {
                languageDropdown.value = i;
                break;
            }
        }

        RefreshText();
    }

    public void OnDefaultButtonDown()
    {
        ShowConfig(SettingsManager.instance.defaultConfig);

        RefreshText();

        controlManagerSettingMenu.OnDefaultButtonDown();
    }

    private void EnableGroupMenu()
    {
        groupMenu.Init();
        EnableUISettingsElements();
    }

    private void OnEnable()
    {
        availableResolutions = SettingsManager.instance.GetAvailableResolutions();
        availableFramerate = SettingsManager.instance.GetAvailableRefreshRate();

        InitOptions();

        ShowConfig(SettingsManager.instance.currentConfig);

        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(true);
        }

        warningNoApplyPanel.SetActive(false);

        this.InvokeWaitAFrame(nameof(EnableGroupMenu));

        mainMenu.SetActive(false);
        isEnable = true;
    }
}
