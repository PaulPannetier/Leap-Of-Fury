using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Lampe : MonoBehaviour
{
    [SerializeField] private float avgIntensity;
    [SerializeField] private float intensityVariation = 1f;
    [SerializeField] private float intensityFrequency = 1f;
    [SerializeField] private float avgWindForce = 1000f;
    [SerializeField] private float windFrequency = 1f;
    public float innerRadius
    {
        get => lamp.pointLightInnerRadius;
        set
        {
            lamp.pointLightInnerRadius = value;
        }
    }
    public float radius
    {
        get => lamp.pointLightOuterRadius;
        set
        {
            lamp.pointLightOuterRadius = value;
        }
    }

    private Light2D lamp;
    private float noiseIndexIntensity;
    private float noiseIndexWind;
    private Rigidbody2D rb;
    private Transform lastChild;

    private void Awake()
    {
        lamp = GetComponentInChildren<Light2D>();
        Transform child = transform.GetChild(0);
        lastChild = child.GetChild(child.childCount - 1);
        rb = lastChild.GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        lamp.intensity = avgIntensity;
        lamp.pointLightInnerRadius = innerRadius;
        lamp.pointLightOuterRadius = radius;
        noiseIndexIntensity = Random.Rand(0f, 50f);
        noiseIndexWind = Random.Rand(0f, 50f);
    }

    private void Update()
    {
        float noiseValue = Random.PerlinNoise(noiseIndexIntensity, 0f) * intensityVariation;
        lamp.intensity = avgIntensity + noiseValue;

        noiseValue = Random.PerlinNoise(noiseIndexWind, 0f) * avgWindForce;
        Vector2 force = (noiseValue * Time.deltaTime) * (lastChild.position - transform.position).ToVector2().NormalVector();
        rb.AddForce(force);

        noiseIndexIntensity += Time.deltaTime * intensityFrequency;
        noiseIndexWind += Time.deltaTime * windFrequency;
    }

    private void OnValidate()
    {
        avgIntensity = Mathf.Max(0f, avgIntensity);
        intensityFrequency = Mathf.Max(0f, intensityFrequency);
        intensityVariation = Mathf.Max(0f, intensityVariation);
        avgWindForce = Mathf.Max(0f, avgWindForce);
        windFrequency = Mathf.Max(0f, windFrequency);
    }
}
