#if UNITY_EDITOR

using System.Diagnostics;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private Transform _transform;

    public int nbTest = 100000;
    public bool test = false;

    private void StartTest()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        PerformNoCache();
        stopwatch.Stop();

        print("No cache : " + stopwatch.ElapsedMilliseconds);

        stopwatch = new Stopwatch();
        stopwatch.Start();
        PerformCache();
        stopwatch.Stop();

        print("With cache : " + stopwatch.ElapsedMilliseconds);

    }

    void PerformNoCache()
    {
        for (int i = 0; i < nbTest; i++)
        {
            GameObject go = GetComponent<EventManager>().gameObject;
        }
    }

    void PerformCache()
    {
        EventManager eventManager = GetComponent<EventManager>();
        for (int i = 0; i < nbTest; i++)
        {
            GameObject go = eventManager.gameObject;
        }
    }

    private void OnValidate()
    {
        if(test)
        {
            test = false;
            StartTest();
        }
    }
}

#endif
