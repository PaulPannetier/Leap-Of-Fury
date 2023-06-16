using System;
using System.Threading.Tasks;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    public const string saveFileExtension = ".partyGame";
    [HideInInspector] public ConfigurationData defaultConfig;
    [HideInInspector] public ConfigurationData currentConfig;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;

        defaultConfig = new ConfigurationData(new Vector2Int(1920,1080), 60, "english", FullScreenMode.FullScreenWindow);
    }

    public void LoadSettings()
    {
        if(!Save.ReadJSONData(@"/Save/configuration" + saveFileExtension, out currentConfig))
        {
            currentConfig = defaultConfig.Clone();
        }

        Application.targetFrameRate = currentConfig.targetedFPS;
        Screen.SetResolution(currentConfig.resolusion.x, currentConfig.resolusion.y, currentConfig.windowMode, currentConfig.targetedFPS);
    }

    public async Task SaveCurrentConfiguration()
    {
        await Save.WriteJSONDataAsync(currentConfig, @"/Save/configuration" + saveFileExtension, Callback);
    }

    private void Callback(bool b)
    {
        if (!b)
        {
            if (Application.isPlaying)
            {
                LogManager.instance.WriteLog("Can't save SettingConfiguration in SettingManager.SaveCurrentConfiguration()", currentConfig);
            }
            else
            {
                Console.WriteLine("Can't save SettingConfiguration in SettingManager.SaveCurrentConfiguration()");
            }
        }
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
