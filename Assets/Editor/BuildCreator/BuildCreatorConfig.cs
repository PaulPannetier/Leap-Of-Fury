using UnityEngine;

public class BuildCreatorConfig : ScriptableObject
{
    public bool modifyGameplayScene = true;
    [Tooltip("The gameplay scene to modify before build")]
    public Object[] gameplayScenes;

    [Tooltip("The non gameplay scene to add to the build")]
    public Object[] otherSceneTobuild;
    public Object firstSceneToPlay;

    public bool performBuild = true;
    public bool developpementBuild = true;
    public bool copySaveDirectory = true;
    public bool setDefaultSettingAndConfig = true;
}
