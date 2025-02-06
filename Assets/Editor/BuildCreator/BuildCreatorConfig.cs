#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public class BuildCreatorConfig : ScriptableObject
{
    public enum BuildPlateform
    {
        Windows,
        Linux,
        MacOSIntel,
        MacOSAppleSilicon,
        MacOSUniversal
    }

    public string companyName;
    public string gameName;
    public string version;

    [Tooltip("The gameplay scene to modify before build")]
    public Object[] gameplayScenes;

    [Tooltip("The non gameplay scene to add to the build")]
    public Object[] otherSceneTobuild;
    public Object firstSceneToPlay;

    public bool modifyGameplayScene = true;
    public bool performBuild = true;
    public BuildPlateform buildPlateform = BuildPlateform.Windows;
    public bool useIL2CPPCompilation = true;
    public bool developmentBuild = true;
    public ManagedStrippingLevel managedStrippingLevel = ManagedStrippingLevel.Disabled;
    public bool copySaveDirectory = true;
    public bool setDefaultSettingAndInput = true;

    private void OnValidate()
    {
        if(buildPlateform == BuildPlateform.MacOSUniversal)
        {
            useIL2CPPCompilation = true;
        }
    }
}

#endif