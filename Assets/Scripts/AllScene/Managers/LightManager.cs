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
            if(Application.isPlaying)
                return _instance;
            return GameObject.FindAnyObjectByType<LightManager>();
#else
            return _instance;
#endif
        }
        private set
        {
            _instance = value;
        }
    }

#if UNITY_EDITOR

    [SerializeField] private Color globalLightColor = Color.white;
    [SerializeField] private float globalLightIntensity = 1f;

#endif

    private Light2D[] _lights;
    [HideInInspector] public Light2D[] lights
    {
        get
        {
#if UNITY_EDITOR
            return GameObject.FindObjectsByType<Light2D>(FindObjectsSortMode.None).Where((Light2D light) => light.lightType != Light2D.LightType.Global).ToArray();
#else
            return _lights; 
#endif
        }

        private set
        {
            _lights = value;
        }
    }
    public Light2D globalLight => GlobalLight.globalLight;

    private void Awake()
    {
        instance = this;
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
        if (instance == this)
            instance = null;
        EventManager.instance.callbackOnMapChanged -= OnMapLoaded;
    }

    #region OnValidate

#if UNITY_EDITOR

    private void OnValidate()
    {
        globalLightIntensity = Mathf.Max(globalLightIntensity, 0f);
        Light2D globalLight = this.globalLight;
        if(globalLight != null)
        {
            globalLight.color = globalLightColor;
            globalLight.intensity = globalLightIntensity;
        }
    }

#endif

    #endregion
}
