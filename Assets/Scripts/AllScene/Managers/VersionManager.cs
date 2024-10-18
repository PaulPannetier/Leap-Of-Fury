using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class VersionManager
{
    private static Dictionary<string, Func<string>> conversionFunction = new Dictionary<string, Func<string>>()
    {
        { "0.0", ConvertV00To01 }
    };

    #region ConvertionFunction

    private static string ConvertV00To01()
    {
        return "0.1";
    }

    #endregion

    public static string version {  get; private set; }

    public static void WriteBuildVersion(string version, string saveBuildPath)
    {
        VersionData versionData = new VersionData(version);
        Save.WriteJSONData(versionData, Path.Combine(saveBuildPath, version + SettingsManager.saveFileExtension));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Start()
    {
        if(!Save.ReadJSONData<VersionData>($"/Save/version" + SettingsManager.saveFileExtension, out VersionData versionData))
        {
            Debug.LogWarning("version file unfound");
            return;
        }

        version = versionData.version;

        //Convert Save directory
        while(conversionFunction.ContainsKey(version))
        {
            Func<string> convertFunc = conversionFunction[version];
            version = convertFunc.Invoke();
        }

        string lastVersion = conversionFunction.Keys.Last();
        if (conversionFunction.Keys.Last() != version)
        {
            Debug.LogWarning($"Current version ({version}) can't be convert to the last version ({lastVersion}).");
        }
    }

    private struct VersionData
    {
        public string version;

        public VersionData(string version)
        { 
            this.version = version;
        }
    }
}
