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

    [SerializeField] private Slider masterSlider;
    [SerializeField] private TMP_Text masterText;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TMP_Text musicText;
    [SerializeField] private Slider soundFXSlider;
    [SerializeField] private TMP_Text soundFXText;
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
    }

    public void OnApplyButtonDown()
    {
        Vector2Int resolution = availableResolutions[resolutionDropdown.value];
        RefreshRate targetedFPS = availableFramerate[framerateDropdown.value];
        string language = LanguageManager.instance.availableLanguage[languageDropdown.value];
        FullScreenMode windowMode = convertIntToFullScreenMode[windowModeDropdown.value];

        SettingsManager.ConfigurationData configurationData = new SettingsManager.ConfigurationData(masterSlider.value, musicSlider.value, soundFXSlider.value, resolution, targetedFPS, language, windowMode, false, vSynchToggle.isOn);

        SettingsManager.instance.SetCurrentConfig(configurationData);

        ControlManagerSettingMenu.instance.OnApplyButtonDown();
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

        ControlManagerSettingMenu.instance.OnDefaultButtonDown();
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
