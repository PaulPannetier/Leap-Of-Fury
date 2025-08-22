using UnityEngine.Rendering.Universal;
using UnityEngine;
#if UNITY_EDITOR
using System.Linq;
#endif

public class LightManager : MonoBehaviour
{
    private static LightManager _instance;
    public static LightManager instance
    {
        get
        {
#if UNITY_EDITOR
            if(!Application.isPlaying)
                return GameObject.FindAnyObjectByType<LightManager>();
            return _instance;
#else
            return _instance;
#endif
        }
        private set
        {
            _instance = value;
        }
    }

    private Light2D[] _lights;
    [HideInInspector] public Light2D[] lights
    {
        get
        {
#if UNITY_EDITOR
            if(!Application.isPlaying)
                return GameObject.FindObjectsByType<Light2D>(FindObjectsSortMode.None).Where((Light2D light) => light.lightType != Light2D.LightType.Global).ToArray();
            return _lights;
#else
            return _lights;
#endif
        }

        private set
        {
            _lights = value;
        }
    }

    private Light2D _globalLight;
    public Light2D globalLight
    {
        get
        {
#if UNITY_EDITOR
            if(!Application.isPlaying)
                return GetComponent<Light2D>();
            return _globalLight;
#else
            return _globalLight;
#endif
        }
        private set { _globalLight = value; }
    }

    private void Awake()
    {
        instance = this;
        globalLight = GetComponent<Light2D>();
    }

    private void Start()
    {
        EventManager.instance.callbackOnMapChanged += OnMapLoaded;
    }

    private void OnMapLoaded(LevelMapData levelMapData)
    {
        lights = levelMapData.GetComponentsInChildren<Light2D>();
    }

    private void OnDestroy()
    {
        EventManager.instance.callbackOnMapChanged -= OnMapLoaded;
    }
}
