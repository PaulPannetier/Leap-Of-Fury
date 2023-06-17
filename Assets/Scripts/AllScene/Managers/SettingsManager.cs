using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        defaultConfig = new ConfigurationData(new Vector2Int(1920, 1080), 60, "english", FullScreenMode.FullScreenWindow);
        LoadSettings();
    }

    public void LoadSettings()
    {
        if(!Save.ReadJSONData(@"/Save/configuration" + saveFileExtension, out ConfigurationData tmp))
        {
            currentConfig = defaultConfig.Clone();
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
        Application.targetFrameRate = currentConfig.targetedFPS;
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

    public int[] GetAvailableRefreshRate()
    {
        Resolution[] resolutions = Screen.resolutions;
        List <int> res = new List<int>();

        foreach (Resolution resolution in resolutions)
        {
            res.Add(resolution.refreshRate);
        }
        res = res.Distinct().ToList();
        res.Sort();
        return res.ToArray();
    }

    [Serializable]
    public struct ConfigurationData
    {
        public Vector2Int resolusion;
        public int targetedFPS;
        public string language;
        public FullScreenMode windowMode;

        public ConfigurationData(in Vector2Int resolusion, int targetedFPS, string language, FullScreenMode windowMode)
        {
            this.resolusion = resolusion;
            this.targetedFPS = targetedFPS;
            this.language = language;
            this.windowMode = windowMode;
        }

        public ConfigurationData Clone()
        {
            return new ConfigurationData(resolusion, targetedFPS, language, windowMode);
        }
    }
}
