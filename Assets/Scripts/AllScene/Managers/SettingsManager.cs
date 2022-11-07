using UnityEngine;
using System;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    public const string saveFileExtension = ".partyGame";

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }
}
