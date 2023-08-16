using UnityEngine;

public class BuildCreatorConfig : ScriptableObject
{
    public bool modifyGameplayScene = true;
    [Tooltip("The gameplay scene to modify before build")]
    public Object[] gameplayScenes;

    public bool performBuild = true;
    [Tooltip("The non gameplay scene to add to the build")]
    public Object[] otherSceneTobuild;

    public bool copySaveDirectory = true;
    public bool setDefaultSettingAndConfig = true;
}
