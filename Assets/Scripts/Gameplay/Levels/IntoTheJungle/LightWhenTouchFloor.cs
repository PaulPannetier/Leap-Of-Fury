using UnityEngine;


public class LightWhenTouchFloor : MonoBehaviour
{
    private Movement movement;
    private float groundLightMaxIntensity, rightLightMaxIntensity, leftLightMaxIntensity;

    [SerializeField] private UnityEngine.Rendering.Universal.Light2D lightGround;
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D lightRight;
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D lightLeft;
    [Tooltip("temps pour passer de lumi�re etteinte a allumer en %age/sec"), SerializeField] private float intensityLerp;

    private void Start()
    {
        movement = transform.parent.GetComponent<ToricObject>().original.GetComponent<Movement>();
        groundLightMaxIntensity = lightGround.intensity;
        rightLightMaxIntensity = lightRight.intensity;
        leftLightMaxIntensity = lightLeft.intensity;
    }

    private void Update()
    {
        lightGround.intensity = Mathf.MoveTowards(lightGround.intensity, movement.isGrounded ? groundLightMaxIntensity : 0f, intensityLerp * groundLightMaxIntensity * Time.deltaTime);
        lightRight.intensity = Mathf.MoveTowards(lightRight.intensity, movement.onRightWall ? rightLightMaxIntensity : 0f, intensityLerp * rightLightMaxIntensity * Time.deltaTime);
        lightLeft.intensity = Mathf.MoveTowards(lightLeft.intensity, movement.onLeftWall ? leftLightMaxIntensity : 0f, intensityLerp * leftLightMaxIntensity * Time.deltaTime);
    }

    private void OnValidate()
    {
        intensityLerp = Mathf.Max(0.00001f, intensityLerp);
    }
}
