#if UNITY_EDITOR

using UnityEngine;
using System;

public class TestScript : MonoBehaviour
{
    private void Start()
    {
        EventManager.instance.callbackPreUpdate += PreUpdate;
    }

    private void PreUpdate()
    {
        GetComponent<CustomPlayerInput>().x = 1f;
        GetComponent<CustomPlayerInput>().rawX = 1;
    }
}

#endif
