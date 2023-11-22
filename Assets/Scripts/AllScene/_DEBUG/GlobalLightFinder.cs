using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlobalLightFinder : MonoBehaviour
{
    [SerializeField] private Light2D[] globalsLights;
    [SerializeField] private Light2D[] lights;
    [SerializeField] private bool search;
    [SerializeField] private bool autoSearch;

    private void Update()
    {
        if(autoSearch)
        {
            SearchLights();
        }
    }

    private void SearchLights()
    {
        Light2D[] tmpLights = GameObject.FindObjectsOfType<Light2D>();

        List<Light2D> globals = new List<Light2D>(), nonGlobals = new List<Light2D>();

        foreach (Light2D light in tmpLights)
        {
            if(light.lightType == Light2D.LightType.Global)
                globals.Add(light);
            else
                nonGlobals.Add(light);
        }
        globalsLights = globals.ToArray();
        lights = nonGlobals.ToArray();
    }

    private void OnValidate()
    {
        if(search)
        {
            search = false;
            SearchLights();
        }
    }
}
