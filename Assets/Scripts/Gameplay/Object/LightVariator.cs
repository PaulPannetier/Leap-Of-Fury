using UnityEngine;

[RequireComponent(typeof(UnityEngine.Rendering.Universal.Light2D))]
public class LightVariator : MonoBehaviour
{
    public bool enableBehaviour = true;

    private new UnityEngine.Rendering.Universal.Light2D light;
    private float noiseIndexIntensity, yNoise;

    public float avgIntensity;
    [SerializeField] private float intensityAmplitude;
    [SerializeField] private float intensityFrequency = 1f;

    private void Awake()
    {
        light = GetComponent<UnityEngine.Rendering.Universal.Light2D>();
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
        light.intensity = avgIntensity + noiseValue;
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

    private void OnValidate()
    {
        light = GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        light.intensity = avgIntensity;
    }
}
