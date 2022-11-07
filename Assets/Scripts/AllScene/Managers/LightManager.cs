using UnityEngine.Experimental.Rendering.Universal;
using System.Linq;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public static LightManager instance;

    [HideInInspector] public Light2D[] globalLights;
    [HideInInspector] public Light2D[] lights;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        Light2D[] allLights = GetComponentsInChildren<Light2D>();
        globalLights = allLights.Where((Light2D l) => l.lightType == Light2D.LightType.Global).ToArray();
        lights = allLights.Where((Light2D l) => l.lightType != Light2D.LightType.Global).ToArray();
    }
}
