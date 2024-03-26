using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class LightVariator : MonoBehaviour
{
    public bool enableBehaviour = true;

    private Light2D currentLight;
    private float noiseIndexIntensity, yNoise;

    public float avgIntensity;
    [SerializeField] private float intensityAmplitude;
    [SerializeField] private float intensityFrequency = 1f;

    private void Awake()
    {
        currentLight = GetComponent<Light2D>();
        yNoise = Random.Rand();
    }

    private void Start()
    {
        PauseManager.instance.callBackOnPauseDisable += Enable;
        PauseManager.instance.callBackOnPauseEnable += Disable;
    }

    private void Update()
    {
        if (!enableBehaviour)
            return;
        float noiseValue = Random.PerlinNoise(noiseIndexIntensity, yNoise) * intensityAmplitude;
        currentLight.intensity = avgIntensity + noiseValue;
        noiseIndexIntensity += Time.deltaTime * intensityFrequency;
    }

    private void Disable()
    {
        enableBehaviour = false;
    }

    private void Enable()
    {
        enableBehaviour = true;
    }

    private void OnDestroy()
    {
        PauseManager.instance.callBackOnPauseEnable -= Disable;
        PauseManager.instance.callBackOnPauseDisable -= Enable;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        currentLight = GetComponent<Light2D>();
        currentLight.intensity = avgIntensity;
    }

#endif
}
