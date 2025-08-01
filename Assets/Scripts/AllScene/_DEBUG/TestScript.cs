#if UNITY_EDITOR

using UnityEngine;
using System;

public class TestScript : MonoBehaviour
{
    private void Start()
    {
        LogManager.instance.ClearLog();
        LogManager.instance.AddLog("This is my log");
        //log();
    }

    private void log()
    {
        LogManager.instance.AddLog("This is my log2");
        LogManager.instance.AddLog("This is my log3", new object[] { "Hello", new Vector2(1, 7)});

        throw new DivideByZeroException("Cannot divide by 0");
    }

}

#endif
