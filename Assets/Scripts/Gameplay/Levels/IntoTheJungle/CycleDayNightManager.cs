
using UnityEngine;

public class CycleDayNightManager : MonoBehaviour
{
    public static CycleDayNightManager instance;

    private int counterDay;

    [SerializeField, Tooltip("Le nombre de jour d'affil�")] private int nbDay;
    [SerializeField, Tooltip("Le nombre de nuit d'affil�")] private int nbNight;
    [SerializeField] private bool startLevelAtDay = true;
    [SerializeField] private float globalLightIntensityAtDay = 1;
    [SerializeField] private float otherLightIntensityAtDay = 0.1f;
    [SerializeField] private float globalLightIntensityAtNight = 0.1f;
    [SerializeField] private float otherLightIntensityAtNight = 1.2f;

    [HideInInspector] public bool isDay;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        isDay = startLevelAtDay;
    }

    private void Start()
    {
        EventManager.instance.callbackOnLevelRestart += Restart;
        counterDay = 0;
        ActivateDay(startLevelAtDay);
    }

    private void Restart(string levelName)
    {
        counterDay++;
        if(isDay)
        {
            if(counterDay >= nbDay)
            {
                ActivateDay(false);
                counterDay = 0;
            }
        }
        else
        {
            if (counterDay >= nbNight)
            {
                ActivateDay(true);
                counterDay = 0;
            }
        }
    }

    private void ActivateDay(bool isDay)
    {
        this.isDay = isDay;

        float globalLightIntensity = isDay ? globalLightIntensityAtDay : globalLightIntensityAtNight;
        float otherLightIntensity = isDay ? otherLightIntensityAtDay : otherLightIntensityAtNight;
        ApplyIntensityWithSettings(globalLightIntensity, otherLightIntensity);
        EnableLightVariator(!isDay);

        void ApplyIntensityWithSettings(float globalLightIntensity, float otherLightIntensity)
        {
            foreach (UnityEngine.Rendering.Universal.Light2D light in LightManager.instance.globalLights)
            {
                light.intensity = globalLightIntensity;
            }
            foreach (UnityEngine.Rendering.Universal.Light2D light in LightManager.instance.lights)
            {
                light.intensity = otherLightIntensity;
            }
        }

        void EnableLightVariator(bool enable)
        {
            for (int i = 0; i < LightManager.instance.lights.Length; i++)
            {
                UnityEngine.Rendering.Universal.Light2D light = LightManager.instance.lights[i];
                if (light.TryGetComponent(out LightVariator lv))
                {
                    lv.enableBehaviour = enable;
                }
            }
        }
    }
}
