using UnityEngine.Rendering.Universal;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public static LightManager instance;

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
}
