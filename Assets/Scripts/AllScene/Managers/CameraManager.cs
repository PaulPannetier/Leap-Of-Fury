using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    public readonly Vector2 cameraSize = new Vector2(32f, 18f);
    public Camera mainCamera { get; private set; }

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        mainCamera = GetComponent<Camera>();
    }
}
