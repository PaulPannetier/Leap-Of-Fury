using UnityEngine;

public class SelectionCharOldSceneData : OldSceneData
{
    public CharData[] charData {  get; private set; }

    public SelectionCharOldSceneData(CharData[] charData) : base("Selection Char")
    {
        this.charData = charData;
    }
}