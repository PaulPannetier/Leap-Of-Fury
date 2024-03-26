using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlobalLight : MonoBehaviour
{
    private static Light2D _globalLight;
    public static Light2D globalLight
    {
        get
        {
#if UNITY_EDITOR
            GlobalLight glogalLightGO = GameObject.FindObjectOfType<GlobalLight>();
            _globalLight = glogalLightGO != null ? glogalLightGO.GetComponent<Light2D>() : null;
            return _globalLight;
#else
            return _globalLight;
#endif
        }
    }

    private void Awake()
    {
        _globalLight = GetComponent<Light2D>();
    }
}
