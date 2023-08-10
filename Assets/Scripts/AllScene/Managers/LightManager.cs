using System.Linq;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public static LightManager instance;

    [HideInInspector] public UnityEngine.Rendering.Universal.Light2D[] globalLights;
    [HideInInspector] public UnityEngine.Rendering.Universal.Light2D[] lights;

    private void Awake()
    {
        instance = this;

        UnityEngine.Rendering.Universal.Light2D[] allLights = GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>();
        globalLights = allLights.Where((UnityEngine.Rendering.Universal.Light2D l) => l.lightType == UnityEngine.Rendering.Universal.Light2D.LightType.Global).ToArray();
        lights = allLights.Where((UnityEngine.Rendering.Universal.Light2D l) => l.lightType != UnityEngine.Rendering.Universal.Light2D.LightType.Global).ToArray();
    }
}
