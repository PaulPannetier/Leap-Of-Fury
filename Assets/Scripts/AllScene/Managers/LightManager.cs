using System.Linq;
using UnityEngine.Rendering.Universal;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public static LightManager instance;

    [HideInInspector] public Light2D[] globalLights;
    [HideInInspector] public Light2D[] lights;

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
        Light2D[] allLights = levelMapData.GetComponentsInChildren<Light2D>();
        globalLights = allLights.Where((Light2D l) => l.lightType == Light2D.LightType.Global).ToArray();
        lights = allLights.Where((Light2D l) => l.lightType != Light2D.LightType.Global).ToArray();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
        EventManager.instance.callbackOnMapChanged -= OnMapLoaded;
    }
}
