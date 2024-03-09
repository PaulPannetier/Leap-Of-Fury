using UnityEngine;

public class LevelOldSceneData : OldSceneData
{
    public CharData[] charData;

    public LevelOldSceneData(string sceneName, CharData[] charData) : base(sceneName)
    {
        this.charData = charData;
    }
}
