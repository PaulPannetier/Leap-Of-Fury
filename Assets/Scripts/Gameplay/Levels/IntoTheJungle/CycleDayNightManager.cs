using UnityEngine.Experimental.Rendering.Universal;
using System.Collections;
using UnityEngine;

public class CycleDayNightManager : MonoBehaviour
{
    private float[] avgLightsIntensity;
    private float lastChangeCycle;
    private bool isDay = true;

    [SerializeField, Range(0f, 2f), Tooltip("Le décalage en pourcentage de durée (0 => début du jour, 0.5 milieu du jour, 1 => début de nuit, 1.5 => milieu de nuit et 2 => fin de nuit)")]
    private float timeOffset = 0.5f;
    [SerializeField] private float dayDuration, nightDuration;
    [SerializeField, Tooltip("luminosity when dayLuminosityOverTime(t) == 0")] private float dayMinLuminosity;
    [SerializeField, Tooltip("luminosity when dayLuminosityOverTime(t) == 1")] private float dayMaxLuminosity;
    [SerializeField, Tooltip("luminosity when nightLuminosityOverTime(t) == 0")] private float nightMinLuminosity;
    [SerializeField, Tooltip("luminosity when nightLuminosityOverTime(t) == 1")] private float nightMaxLuminosity;
    [SerializeField] private AnimationCurve dayLuminosityOverTime, nightLuminosityOverTime;
    [SerializeField, Tooltip("en %age/sec")] private float nonGlobalLightLerp;

    private void Start()
    {
        isDay = timeOffset >= 0f && timeOffset < 1f;
        lastChangeCycle = Time.time - Mathf.Clamp01(timeOffset) * dayDuration - Mathf.Clamp01(timeOffset - 1f) * nightDuration;

        avgLightsIntensity = new float[LightManager.instance.lights.Length];
        for (int i = 0; i < LightManager.instance.lights.Length; i++)
        {
            Light2D light = LightManager.instance.lights[i];
            if (light.TryGetComponent(out LightVariator lv))
            {
                lv.enableBehaviour = !isDay;
                avgLightsIntensity[i] = lv.avgIntensity;
            }
            else
            {
                avgLightsIntensity[i] = light.intensity;
            }
            light.intensity = isDay ? 0f : avgLightsIntensity[i];
        }
    }

    private void Update()
    {
        if(isDay)
        {
            if (Time.time - lastChangeCycle > dayDuration)
            {
                isDay = !isDay;
                lastChangeCycle = Time.time;
                ApplyIntensityWithSettings(nightMinLuminosity, nightMaxLuminosity, 0f, nightLuminosityOverTime);
                StartCoroutine(TriggerLights(true));
            }
            else
            {
                ApplyIntensityWithSettings(dayMinLuminosity, dayMaxLuminosity, (Time.time - lastChangeCycle) / dayDuration, dayLuminosityOverTime);
            }
        }
        else
        {
            if (Time.time - lastChangeCycle > nightDuration)
            {
                isDay = !isDay;
                lastChangeCycle = Time.time;
                ApplyIntensityWithSettings(dayMinLuminosity, dayMaxLuminosity, 0f, dayLuminosityOverTime);
                StartCoroutine(TriggerLights(false));
            }
            else
            {
                ApplyIntensityWithSettings(nightMinLuminosity, nightMaxLuminosity, (Time.time - lastChangeCycle) / nightDuration, nightLuminosityOverTime);
            }
        }

        void ApplyIntensityWithSettings(float minLuminosity, float maxLuminosity, float percentage, AnimationCurve animationCurve)
        {
            float intensity = Mathf.Lerp(minLuminosity, maxLuminosity, animationCurve.Evaluate(Mathf.Clamp01(percentage)));
            foreach (Light2D light in LightManager.instance.globalLights)
            {
                light.intensity = intensity;
            }
        }
    }

    private IEnumerator TriggerLights(bool enable = true)
    {
        if(!enable)
        {
            EnableLightVariator();
        }

        float duration = 1f / nonGlobalLightLerp;
        float timeBeg = Time.time;
        while (Time.time - timeBeg <= duration)
        {
            float percent = (Time.time - timeBeg) / duration;
            for (int i = 0; i < LightManager.instance.lights.Length; i++)
            {
                Light2D light = LightManager.instance.lights[i];
                float target = enable ? avgLightsIntensity[i] : 0f;
                float beg = enable ? 0f : avgLightsIntensity[i];
                light.intensity = Mathf.Lerp(beg, target, percent);
            }
            yield return null;
        }

        if (enable)
        {
            EnableLightVariator();
        }

        void EnableLightVariator()
        {
            for (int i = 0; i < LightManager.instance.lights.Length; i++)
            {
                Light2D light = LightManager.instance.lights[i];

                if (light.TryGetComponent(out LightVariator lv))
                {
                    lv.enableBehaviour = enable;
                }
            }
        }
    }

    private void OnValidate()
    {
        nonGlobalLightLerp = Mathf.Max(0f, nonGlobalLightLerp);
    }
}
