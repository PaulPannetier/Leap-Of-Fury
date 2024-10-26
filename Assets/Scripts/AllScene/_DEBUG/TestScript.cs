#if UNITY_EDITOR

using UnityEngine;

public class TestScript : MonoBehaviour
{
    public void Start()
    {
        LogManager.instance.AddLog("Test log", 64);
    }
}

#endif
