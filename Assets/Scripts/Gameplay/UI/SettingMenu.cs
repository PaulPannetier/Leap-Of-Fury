using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SettingMenu : MonoBehaviour
{
    private static Dictionary<int, FullScreenMode> convertIntToFullScreenMode = new Dictionary<int, FullScreenMode>
    {
        { 0, FullScreenMode.FullScreenWindow },
        { 1, FullScreenMode.Windowed }
    };

    private static Dictionary<FullScreenMode, int> convertFullScreenModeToInt = new Dictionary<FullScreenMode, int>
    {
        { FullScreenMode.FullScreenWindow, 0 },
        { FullScreenMode.Windowed, 1 }
    };

    private static int[] possibleFramerate = new int[]
    {
        30, 60, 75, 90, 120, 144, 165, 240
    };

    private Vector2Int[] availableResolutions;
    private int[] availableFramerate;

    [SerializeField] private TMP_Dropdown windowModeDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown framerateDropdown;
    [SerializeField] private TMP_Dropdown languageDropdown;

    private void Start()
    {
        SettingsManager.ConfigurationData currentConfig = SettingsManager.instance.currentConfig;

        //Window dropdown
        List<TMP_Dropdown.OptionData> windowModeOptions = new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData(LanguageManager.instance.GetText("fullscreenMode"), windowModeDropdown.options[0].image),
            new TMP_Dropdown.OptionData(LanguageManager.instance.GetText("windowedMode"), windowModeDropdown.options[0].image),
        };
        windowModeDropdown.options = windowModeOptions;
        windowModeDropdown.value = convertFullScreenModeToInt[currentConfig.windowMode];

        //resolution dropdown
        availableResolutions = SettingsManager.instance.GetAvailableResolutions();
        int resolutionIndex = 0;
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            if (availableResolutions[i] == currentConfig.resolusion)
            {
                resolutionIndex = i;
                break;
            }
        }

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
        resolutionDropdown.value = resolutionIndex;

        //framerate dropdown
        availableFramerate = SettingsManager.instance.GetAvailableRefreshRate();
        List<TMP_Dropdown.OptionData> framerateOptions = new List<TMP_Dropdown.OptionData>();
        Sprite framerateSprite = framerateDropdown.options[0].image;
        for (int i = 0; i < availableFramerate.Length; i++)
        {
            framerateOptions.Add(new TMP_Dropdown.OptionData(availableFramerate[i].ToString() + " Hz", framerateSprite));
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

    public void OnApplyButtonDown()
    {
        Vector2Int resolution = availableResolutions[resolutionDropdown.value];
        int targetedFPS = availableFramerate[framerateDropdown.value];
        string language = LanguageManager.instance.availableLanguage[languageDropdown.value];
        FullScreenMode windowMode = convertIntToFullScreenMode[windowModeDropdown.value];

        SettingsManager.ConfigurationData configurationData = new SettingsManager.ConfigurationData(resolution, targetedFPS, language, windowMode);

        SettingsManager.instance.SetCurrentConfig(configurationData);
    }

    public void OnDefaultButtonDown()
    {
        SettingsManager.ConfigurationData defaultConfig = SettingsManager.instance.defaultConfig;
        windowModeDropdown.value = convertFullScreenModeToInt[defaultConfig.windowMode];

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            if (availableResolutions[i] == defaultConfig.resolusion)
            {
                resolutionDropdown.value = i;
                break;
            }
        }

        for (int i = 0; i < availableFramerate.Length; i++)
        {
            if (availableFramerate[i] == defaultConfig.targetedFPS)
            {
                framerateDropdown.value = i;
                break;
            }
        }

        for (int i = 0; i < LanguageManager.instance.availableLanguage.Length; i++)
        {
            if(defaultConfig.language == LanguageManager.instance.availableLanguage[i])
            {
                languageDropdown.value = i;
                break;
            }
        }
    }
}
