using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    public const string saveFileExtension = ".partyGame";
    [HideInInspector] public ConfigurationData defaultConfig { get; private set; }
    [HideInInspector] public ConfigurationData currentConfig { get; private set; }

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;

        string defaultLanguage = "English";
        if (Application.systemLanguage == SystemLanguage.French)
        {
            defaultLanguage = "Francais";
        }
        defaultConfig = new ConfigurationData(new Vector2Int(1920, 1080), new RefreshRate { numerator = 60, denominator = 1 }, defaultLanguage, FullScreenMode.FullScreenWindow, true);
        LoadSettings();
    }

    public void LoadSettings()
    {
        if(!Save.ReadJSONData(@"/Save/configuration" + saveFileExtension, out ConfigurationData tmp))
        {
            currentConfig = defaultConfig.Clone();
            currentConfig = new ConfigurationData(currentConfig.resolusion, currentConfig.targetedFPS, currentConfig.language, currentConfig.windowMode, false);
            Save.WriteJSONDataAsync(currentConfig, @"/Save/configuration" + saveFileExtension, (b) => { }).GetAwaiter();
        }
        else
        {
            currentConfig = tmp.Clone();
        }

        ApplyConfiguration();
    }

    private void SaveCurrentConfiguration()
    {
        Save.WriteJSONData(currentConfig, @"/Save/configuration" + saveFileExtension);
    }

    public void SetCurrentConfig(ConfigurationData config)
    {
        currentConfig = config.Clone();
        ApplyConfiguration();
        SaveCurrentConfiguration();
    }

    private void ApplyConfiguration()
    {
        Application.targetFrameRate = ((float)currentConfig.targetedFPS.value).Round();
        Screen.SetResolution(currentConfig.resolusion.x, currentConfig.resolusion.y, currentConfig.windowMode, currentConfig.targetedFPS);
        LanguageManager.instance.currentlanguage = currentConfig.language;
    }

    public Vector2Int[] GetAvailableResolutions()
    {
        Resolution[] resolutions = Screen.resolutions;
        List<Vector2Int> res = new List<Vector2Int>();
        foreach (Resolution resolution in resolutions)
        {
            res.Add(new Vector2Int(resolution.width, resolution.height));
        }
        res = res.Distinct().ToList();
        res.Sort(Vector2IntComparison);
        return res.ToArray();

        int Vector2IntComparison(Vector2Int v1, Vector2Int v2)
        {
            if(v1.x > v2.x)
                return -1;
            if (v1.x < v2.x)
                return 1;
            if (v1.y > v2.y)
                return -1;
            if (v1.y < v2.y)
                return 1;
            return 0;
        }
    }

    public RefreshRate[] GetAvailableRefreshRate()
    {
        Resolution[] resolutions = Screen.resolutions;
        List <RefreshRate> res = new List<RefreshRate>();

        foreach (Resolution resolution in resolutions)
        {
            res.Add(resolution.refreshRateRatio);
        }
        res = res.Distinct().ToList();
        res.Sort(RefreshRateComparison);
        return res.ToArray();

        int RefreshRateComparison(RefreshRate r1, RefreshRate r2)
        {
            return (int)((float)(r2.value - r1.value)).Sign();
        }
    }

    [Serializable]
    public struct ConfigurationData : ICloneable<ConfigurationData>
    {
        public Vector2Int resolusion;
        public RefreshRate targetedFPS;
        public string language;
        public FullScreenMode windowMode;
        public bool firstTimeLaunch;

        public ConfigurationData(in Vector2Int resolusion, in RefreshRate targetedFPS, string language, FullScreenMode windowMode, bool firstTimeLaunch)
        {
            this.resolusion = resolusion;
            this.targetedFPS = targetedFPS;
            this.language = language;
            this.windowMode = windowMode;
            this.firstTimeLaunch = firstTimeLaunch;
        }

        public ConfigurationData Clone()
        {
            return new ConfigurationData(resolusion, targetedFPS, language, windowMode, firstTimeLaunch);
        }
    }
}
