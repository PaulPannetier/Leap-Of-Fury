using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

#if UNITY_EDITOR

    [SerializeField] private bool clearConfiguration = false;

#endif

    private const string configPath = @"/Save/UserSave/configuration" + saveFileExtension;

    public const string saveFileExtension = ".lof";
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
    }

    private void Start()
    {
        LoadSettings();
    }

    private ConfigurationData GetDefaultConfig()
    {
        string defaultLanguage = "English";
        if (Application.systemLanguage == SystemLanguage.French)
        {
            defaultLanguage = "Francais";
        }

        Vector2Int defaultResolusion = GetAvailableResolutions()[0];
        RefreshRate defaultRefreshRate = GetAvailableRefreshRate()[0];

        return new ConfigurationData(1f, 1f, 1f, defaultResolusion, defaultRefreshRate, defaultLanguage, FullScreenMode.FullScreenWindow, true, false, SystemInfo.deviceUniqueIdentifier);
    }

    public void LoadSettings()
    {
        defaultConfig = GetDefaultConfig();

        if (!Save.ReadJSONData(configPath, out ConfigurationData tmp))
        {
            currentConfig = defaultConfig.Clone();

            tmp = currentConfig.Clone();
            tmp.firstTimeLaunch = false;

            Save.WriteJSONDataAsync(tmp, configPath, (b) => { }).GetAwaiter();
        }
        else
        {
            if(tmp.deviceID != defaultConfig.deviceID)
            {
                currentConfig = new ConfigurationData(tmp.masterVolume, tmp.musicVolume, tmp.soundFXVolume, defaultConfig.resolusion, defaultConfig.targetedFPS, tmp.language, defaultConfig.windowMode, false, false, defaultConfig.deviceID);
                SaveCurrentConfiguration();
            }
            else
            {
                currentConfig = tmp.Clone();
            }
        }

        ApplyConfiguration();
    }

    private void SaveCurrentConfiguration()
    {
		if (!Save.WriteJSONData(currentConfig, configPath, mkdir:true))
			Debug.LogWarning("Couldn't save new configuration to disk !!!");
    }

    public void SetCurrentConfig(in ConfigurationData config)
    {
        currentConfig = config.Clone();
        ApplyConfiguration();
        SaveCurrentConfiguration();
    }

    private void ApplyConfiguration()
    {
        Application.targetFrameRate = ((float)currentConfig.targetedFPS.value).Round();
        Screen.SetResolution(currentConfig.resolusion.x, currentConfig.resolusion.y, currentConfig.windowMode, currentConfig.targetedFPS);
        QualitySettings.vSyncCount = currentConfig.vSync ? 1 : 0;
        LanguageManager.instance.currentlanguage = currentConfig.language;
        AudioManager.instance.masterVolume = currentConfig.masterVolume;
        AudioManager.instance.musicVolume = currentConfig.musicVolume;
        AudioManager.instance.soundEffectsVolume = currentConfig.soundFXVolume;
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
            bool toAdd = true;
            foreach (RefreshRate rate in res)
            {
                if(rate.value.Round() == resolution.refreshRateRatio.value.Round())
                {
                    toAdd = false;
                    break;
                }
            }
            if(toAdd)
                res.Add(resolution.refreshRateRatio);
        }

        res.Sort(RefreshRateComparison);
        return res.ToArray();

        int RefreshRateComparison(RefreshRate r1, RefreshRate r2)
        {
            return (int)((float)(r2.value - r1.value)).Sign();
        }
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        if(clearConfiguration)
        {
            clearConfiguration = false;
            Save.WriteStringAsync("", configPath, (b) => { }).GetAwaiter();
        }
    }

#endif

    #region Struct

    [Serializable]
    public struct ConfigurationData : ICloneable<ConfigurationData>, IEquatable<ConfigurationData>
    {
        public float masterVolume;
        public float musicVolume;
        public float soundFXVolume;
        [SerializeField] private CustomRefreshRate customRefreshRate;

        public Vector2Int resolusion;
        public RefreshRate targetedFPS
        {
            get
            {
                RefreshRate refreshRate;
                refreshRate.numerator = customRefreshRate.numerator;
                refreshRate.denominator = customRefreshRate.denominator;
                return refreshRate;
            }
        }

        public string language;
        public FullScreenMode windowMode;
        public bool firstTimeLaunch;
        public string deviceID;
        public bool vSync;

        public ConfigurationData(float masterVolume, float musicVolume, float soundFXVolume, in Vector2Int resolusion, in RefreshRate targetedFPS, string language, FullScreenMode windowMode, bool firstTimeLaunch, bool vSync)
        {
            this.masterVolume = masterVolume;
            this.musicVolume = musicVolume;
            this.soundFXVolume = soundFXVolume;
            this.resolusion = resolusion;
            customRefreshRate = new CustomRefreshRate(targetedFPS.numerator, targetedFPS.denominator);
            this.language = language;
            this.windowMode = windowMode;
            this.firstTimeLaunch = firstTimeLaunch;
            deviceID = SystemInfo.deviceUniqueIdentifier;
            this.vSync = vSync;
        }

        public ConfigurationData(float masterVolume, float musicVolume, float soundFXVolume, in Vector2Int resolusion, in RefreshRate targetedFPS, string language, FullScreenMode windowMode, bool firstTimeLaunch, bool vSync, string deviceID)
        {
            this.masterVolume = masterVolume;
            this.musicVolume = musicVolume;
            this.soundFXVolume = soundFXVolume;
            this.resolusion = resolusion;
            customRefreshRate = new CustomRefreshRate(targetedFPS.numerator, targetedFPS.denominator);
            this.language = language;
            this.windowMode = windowMode;
            this.firstTimeLaunch = firstTimeLaunch;
            this.deviceID = deviceID;
            this.vSync = vSync;
        }

        public ConfigurationData Clone()
        {
            return new ConfigurationData(masterVolume, musicVolume, soundFXVolume, resolusion, targetedFPS, language, windowMode, firstTimeLaunch, vSync, deviceID);
        }

        #region Equal

        public bool Equals(ConfigurationData other)
        {
            return masterVolume == other.masterVolume && musicVolume == other.musicVolume && soundFXVolume == other.soundFXVolume && resolusion == other.resolusion &&
                customRefreshRate == other.customRefreshRate && language == other.language && windowMode == other.windowMode && deviceID == other.deviceID && vSync == other.vSync;
        }

        public override bool Equals(object obj)
        {
            if(object.ReferenceEquals(null, obj))
                return false;

            if(obj is ConfigurationData configurationData)
                return this == configurationData;

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return HashCode.Combine(HashCode.Combine(masterVolume, musicVolume, soundFXVolume, resolusion, customRefreshRate), language, windowMode, deviceID, vSync);
        }

        public static bool operator==(ConfigurationData left, ConfigurationData right) => left.Equals(right);
        public static bool operator!=(ConfigurationData left, ConfigurationData right) => !left.Equals(right);

        #endregion

        [Serializable]
        private struct CustomRefreshRate : IEquatable<CustomRefreshRate>
        {
            public uint numerator;
            public uint denominator;

            public CustomRefreshRate(uint numerator, uint denominator)
            {
                this.numerator = numerator;
                this.denominator = denominator;
            }

            #region Equal

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(CustomRefreshRate other)
            {
                if (denominator == 0)
                    return other.denominator == 0u;

                if (other.denominator == 0u)
                    return false;
                return (long)numerator * (long)other.denominator == (long)denominator * (long)other.numerator;
            }

            public override bool Equals(object obj)
            {
                if (object.ReferenceEquals(null, obj))
                    return false;

                if (obj is CustomRefreshRate configurationData)
                    return this == configurationData;

                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int GetHashCode() => HashCode.Combine(numerator, denominator);

            public static bool operator ==(CustomRefreshRate left, CustomRefreshRate right) => left.Equals(right);
            public static bool operator !=(CustomRefreshRate left, CustomRefreshRate right) => !left.Equals(right);

            #endregion
        }
    }

    #endregion
}
