using UnityEngine;

[RequireComponent(typeof(MapColliderData))]
public class ConvoyerBelt : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public bool enableBehaviour = true;
    public float maxSpeed;
    public float speedLerp;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        PauseManager.instance.callBackOnPauseDisable += Disable;
        PauseManager.instance.callBackOnPauseEnable += Enable;
    }

    private void Update()
    {
        spriteRenderer.color = enableBehaviour ? Color.green : Color.red;
        if (!enableBehaviour)
            return;
    }

    private void Disable()
    {
        enableBehaviour = false;
    }

    private void Enable()
    {
        enableBehaviour = true;
    }

    private void OnValidate()
    {
        speedLerp = Mathf.Max(speedLerp, 0f);
    }
}
