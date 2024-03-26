using UnityEngine.Rendering.Universal;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public static LightManager instance;

#if UNITY_EDITOR

    [SerializeField] private Color globalLightColor = Color.white;
    [SerializeField] private float globalLightIntensity = 1f;

#endif

    [HideInInspector] public Light2D[] lights;
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
}
