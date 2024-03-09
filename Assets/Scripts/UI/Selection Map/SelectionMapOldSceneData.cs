using UnityEngine;

public class SelectionMapOldSceneData : OldSceneData
{
    public CharData[] charData { get; private set; }

    public SelectionMapOldSceneData(CharData[] charData) : base("Selection Map")
    {
        this.charData = charData;
    }
}
