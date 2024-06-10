#if UNITY_EDITOR

using System.Diagnostics;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private void Start()
    {
        EventManager.instance.callbackPreUpdate += PreUpdate;
    }

    private void PreUpdate()
    {
        print("PreUpdate");
    }
}

#endif
