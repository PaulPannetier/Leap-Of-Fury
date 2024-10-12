#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public class BuildCreatorConfig : ScriptableObject
{
    public enum CPUArchitecture : byte
    {
        x86 = 0,//32 bits CPU
        x64 = 1 //64 bits CPU
    }

    public string compagnyName;
    public string gameName;
    public string version;

    [Tooltip("The gameplay scene to modify before build")]
    public Object[] gameplayScenes;

    [Tooltip("The non gameplay scene to add to the build")]
    public Object[] otherSceneTobuild;
    public Object firstSceneToPlay;

    public bool modifyGameplayScene = true;
    public bool performBuild = true;
    public bool useIL2CPPCompilation = true;
    public CPUArchitecture architectureCPU = CPUArchitecture.x64;
    public bool developpementBuild = true;
    public ManagedStrippingLevel managedStrippingLevel = ManagedStrippingLevel.Disabled;
    public bool enableFilesSecurity = true;
    public bool copySaveDirectory = true;
    public bool setDefaultSettingAndInput = true;
}

#endif