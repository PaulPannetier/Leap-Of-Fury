using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine;

[RequireComponent(typeof(Light2D))]
public class LightVariator : MonoBehaviour
{
    public bool enableBehaviour = true;

    private new Light2D light;
    private float noiseIndexIntensity, yNoise;

    public float avgIntensity;
    [SerializeField] private float intensityAmplitude;
    [SerializeField] private float intensityFrequency = 1f;

    private void Awake()
    {
        light = GetComponent<Light2D>();
        yNoise = Random.Rand();
    }

    private void Update()
    {
        if (!enableBehaviour)
            return;
        float noiseValue = Random.PerlinNoise(noiseIndexIntensity, yNoise) * intensityAmplitude;
        light.intensity = avgIntensity + noiseValue;
        noiseIndexIntensity += Time.deltaTime * intensityFrequency;
    }
}
