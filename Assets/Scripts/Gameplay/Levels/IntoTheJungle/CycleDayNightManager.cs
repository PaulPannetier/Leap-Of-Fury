using UnityEngine.Rendering.Universal;
using UnityEngine;
using UnityEditor.SceneManagement;

public class CycleDayNightManager : MonoBehaviour
{
    public static CycleDayNightManager instance;

    private int counterDay;

#if UNITY_EDITOR
    [SerializeField] private bool showDayLight, showNightLight;
#endif

    [SerializeField, Tooltip("The number of day levels in a row.")] private int nbDay;
    [SerializeField, Tooltip("The number of night levels in a row.")] private int nbNight;
    public bool startLevelAtDay = true;
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
        counterDay = 0;
    }

    private void Start()
    {
        EventManager.instance.callbackOnLevelRestart += OnLevelRestart;
        EventManager.instance.callbackOnLevelStart += OnLevelStart;
        counterDay = 0;
    }

    private void OnLevelStart(string levelName)
    {
        ActivateDay(startLevelAtDay);
    }

    private void OnLevelRestart(string levelName)
    {
        counterDay++;
        if (isDay)
        {
            if(counterDay >= nbDay)
            {
                ActivateDay(false);
                counterDay = 0;
            }
            else
            {
                ActivateDay(true);
            }
        }
        else
        {
            if (counterDay >= nbNight)
            {
                ActivateDay(true);
                counterDay = 0;
            }
            else
            {
                ActivateDay(false);
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
            LightManager.instance.globalLight.intensity = globalLightIntensity;

            foreach (Light2D light in LightManager.instance.lights)
            {
                light.intensity = otherLightIntensity;
            }
        }

        void EnableLightVariator(bool enable)
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

    private void OnDestroy()
    {
        EventManager.instance.callbackOnLevelRestart -= OnLevelRestart;
        EventManager.instance.callbackOnLevelStart -= OnLevelStart;
    }

    #region OnValidate

#if UNITY_EDITOR

    private void OnValidate()
    {
        if(PrefabStageUtility.GetCurrentPrefabStage() == null)
        {
            if (showDayLight)
            {
                LightManager.instance.globalLight.intensity = globalLightIntensityAtDay;
                foreach (Light2D light in LightManager.instance.lights)
                {
                    light.intensity = otherLightIntensityAtDay;
                }

            }
            else if (showNightLight)
            {
                LightManager.instance.globalLight.intensity = globalLightIntensityAtNight;
                foreach (Light2D light in LightManager.instance.lights)
                {
                    light.intensity = otherLightIntensityAtNight;
                }
            }
        }
    }

#endif

    #endregion
}
