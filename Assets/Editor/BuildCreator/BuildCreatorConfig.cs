#if UNITY_EDITOR

using UnityEngine;

public class BuildCreatorConfig : ScriptableObject
{
    [Tooltip("The gameplay scene to modify before build")]
    public Object[] gameplayScenes;

    [Tooltip("The non gameplay scene to add to the build")]
    public Object[] otherSceneTobuild;
    public Object firstSceneToPlay;

    public bool modifyGameplayScene = true;
    public bool performBuild = true;
    public bool developpementBuild = true;
    public bool advancedDebug = false;
    public bool copySaveDirectory = true;
    public bool setDefaultSettingAndInput = true;
}

#endif