using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class VersionManager
{
    private static List<Tuple<string, Func<string>>> conversionFunctions = new List<Tuple<string, Func<string>>>()
    {
        new Tuple<string, Func<string>>("0.0", ConvertVersion_00_To_01)
    };

    #region ConvertionFunction

    private static string ConvertVersion_00_To_01()
    {
        return "0.1";
    }

    #endregion

    public static string version {  get; private set; }

    public static void WriteBuildVersion(string version, string saveBuildPath)
    {
        VersionData versionData = new VersionData(version);
        string versionDataJSON = Save.Serialize(versionData);
        File.WriteAllText(Path.Combine(saveBuildPath, "UserSave", "version" + SettingsManager.saveFileExtension), versionDataJSON);
        File.WriteAllText(Path.Combine(saveBuildPath, "GameData", "version" + SettingsManager.saveFileExtension), versionDataJSON);
    }

    private static void WriteVersion()
    {
        VersionData versionData = new VersionData(version);
        Save.WriteJSONData(versionData, $"/Save/UserSave/version" + SettingsManager.saveFileExtension);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Start()
    {
        if (Save.ReadJSONData<VersionData>($"/Save/UserSave/version" + SettingsManager.saveFileExtension, out VersionData versionData))
        {
            version = versionData.version;
        }
        else
        {
            version = "0.0";
            WriteVersion();
        }

        if (!Save.ReadJSONData<VersionData>($"/Save/GameData/version" + SettingsManager.saveFileExtension, out VersionData buildVersionData))
        {
            Debug.LogWarning("Build version file unfound in directory ./Save/GameData/");
            return;
        }

        string lastVersion = buildVersionData.version;

        if (lastVersion == version)
            return;

        //Convert Save directory
        int i;
        int indexVersion = -1;
        for (i = 0; i < conversionFunctions.Count; i++)
        {
            if(conversionFunctions[i].Item1 == version)
            {
                indexVersion = i; 
                break;
            }
        }

        if (indexVersion < 0)
        {
            Debug.Log($"Can't find the conversion function for version {version}!");
            return;
        }

        for (i = indexVersion; i < conversionFunctions.Count; i++)
        {
            Tuple<string, Func<string>> conversionFunction = conversionFunctions[i];

            if(conversionFunction.Item1 != version)
            {
                Debug.Log($"Can't convert the version {version}!");
                return;
            }

            version = conversionFunction.Item2.Invoke();
        }

        if (lastVersion != version)
        {
            Debug.LogWarning($"Current version ({version}) can't be convert to the last version ({lastVersion}).");
        }

        WriteVersion();
    }

    [Serializable]
    private struct VersionData
    {
        public string version;

        public VersionData(string version)
        { 
            this.version = version;
        }
    }
}
