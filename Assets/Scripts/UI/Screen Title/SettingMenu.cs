using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    [SerializeField] private TMP_Dropdown windowModeDropdown;
    [SerializeField] private TMP_Text windowModeText;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Text resolutionText;
    [SerializeField] private TMP_Dropdown framerateDropdown;
    [SerializeField] private TMP_Text framerateText;
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private TMP_Text languageText;
    [SerializeField] private Toggle vSynchToggle;
    [SerializeField] private TMP_Text vSynchText;
    [SerializeField] private TMP_Text applyButtonText;
    [SerializeField] private TMP_Text defaultButtonText;
    [SerializeField] private InputManager.GeneralInput echapInput;
    [SerializeField] private GameObject mainMenu;

    private void Awake()
    {
        isEnable = false;
    }

    private void Refresh()
    {
        //Window dropdown
        List<TMP_Dropdown.OptionData> windowModeOptions = new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData(LanguageManager.instance.GetText("fullscreenMode"), windowModeDropdown.options[0].image),
            new TMP_Dropdown.OptionData(LanguageManager.instance.GetText("windowedMode"), windowModeDropdown.options[0].image),
        };
        windowModeDropdown.options = windowModeOptions;
        windowModeText.text = LanguageManager.instance.GetText("windowMode") + " :";

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
        resolutionText.text = LanguageManager.instance.GetText("resolution") + " :";

        //framerate dropdown
        List<TMP_Dropdown.OptionData> framerateOptions = new List<TMP_Dropdown.OptionData>();
        Sprite framerateSprite = framerateDropdown.options[0].image;
        for (int i = 0; i < availableFramerate.Length; i++)
        {
            framerateOptions.Add(new TMP_Dropdown.OptionData(availableFramerate[i].value.Round().ToString() + " Hz", framerateSprite));
        }
        framerateDropdown.options = framerateOptions;
        framerateText.text = LanguageManager.instance.GetText("framerate") + " :";

        //language dropdown
        List<TMP_Dropdown.OptionData> languageOptions = new List<TMP_Dropdown.OptionData>();
        Sprite languageSprite = languageDropdown.options[0].image;
        foreach (string language in LanguageManager.instance.availableLanguage)
        {
            languageOptions.Add(new TMP_Dropdown.OptionData(language, languageSprite));
        }
        languageDropdown.options = languageOptions;
        languageText.text = LanguageManager.instance.GetText("language") + " :";

        //VSynch
        vSynchText.text = LanguageManager.instance.GetText("vsync") + " :";

        //buttons
        applyButtonText.text = LanguageManager.instance.GetText("applyOptionButton");
        defaultButtonText.text = LanguageManager.instance.GetText("defaultOptionButton");
    }

    public void OnApplyButtonDown()
    {
        Vector2Int resolution = availableResolutions[resolutionDropdown.value];
        RefreshRate targetedFPS = availableFramerate[framerateDropdown.value];
        string language = LanguageManager.instance.availableLanguage[languageDropdown.value];
        FullScreenMode windowMode = convertIntToFullScreenMode[windowModeDropdown.value];

        SettingsManager.ConfigurationData configurationData = new SettingsManager.ConfigurationData(resolution, targetedFPS, language, windowMode, false, vSynchToggle.isOn);

        SettingsManager.instance.SetCurrentConfig(configurationData);

        ControlManagerSettingMenu.instance.OnApplyButtonDown();

        Refresh();
    }

    private void ShowConfig(in SettingsManager.ConfigurationData configurationData)
    {
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
    }

    public void OnDefaultButtonDown()
    {
        ShowConfig(SettingsManager.instance.defaultConfig);

        Refresh();

        ControlManagerSettingMenu.instance.OnDefaultButtonDown();
    }

    private void OnEnable()
    {
        availableResolutions = SettingsManager.instance.GetAvailableResolutions();
        availableFramerate = SettingsManager.instance.GetAvailableRefreshRate();

        ShowConfig(SettingsManager.instance.currentConfig);

        Refresh();

        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(true);
        }
        mainMenu.SetActive(false);
        isEnable = true;
    }

    private void Update()
    {
        if (!isEnable)
            return;

        if(echapInput.IsPressedDown())
        {
            isEnable = false;
            mainMenu.SetActive(true);
            mainMenu.GetComponentInChildren<SelectableUIGroup>().enableBehaviour = true;
            gameObject.SetActive(false);
        }
    }
}
