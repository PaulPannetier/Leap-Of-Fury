#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] private bool addLog = false;

    private void OnValidate()
    {
        if(addLog)
        {
            addLog = false;
            LogManager.instance.ClearLog();
            if(Application.isPlaying)
            {
                string param1 = "param1";
                float param2 = 3.1415f;
                InputKey param3 = InputKey.Escape;
                Vector3 param4 = new Vector3(1.25f, 54.1f, 54.8f);
                MyClass myClass = new MyClass();
                List<float> floatList = new List<float>() { 2.1f, 68.0f };
                float[] floatArray = new float[3] { 1.0f, 3.14f, 8.4f };
                Dictionary<string, Vector2> dict = new Dictionary<string, Vector2>()
                {
                    { "bonjour", Vector2.one },
                    { "aurevoir", Vector2.zero }
                };

                LogManager.instance.AddLog("This is a test log", param1, param2, param3, param4, myClass, floatList, dict);
            }
        }
    }

    private class MyClass
    {
        public int publicInt = 5;
        private int privateInt = 65;
        public List<float> floatList = new List<float>() { 2.1f, 68.0f};
    }
}

#endif
