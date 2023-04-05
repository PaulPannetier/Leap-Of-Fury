using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isAbuild = false;
    [SerializeField][Range(0, 10)] private int pixelQuality = 10;
    [SerializeField] private Vector2Int maxResolution = new Vector2Int(1920, 1080);
    [Tooltip("info")] public Vector2Int currentResolution = new Vector2Int(1920, 1080);

    private int[] divisor = new int[11] { 60, 30, 20, 15, 12, 10, 6, 5, 3, 2, 1 };
    private PixelPerfectCamera pixelPerfectCam;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        pixelPerfectCam = Camera.main.GetComponent<PixelPerfectCamera>();
        transform.parent = null;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Application.targetFrameRate = 165;
        Random.SetRandomSeed();
    }

    private void SetPixelQuality()
    {
        if (pixelPerfectCam == null)
            return;
        pixelPerfectCam.refResolutionX = maxResolution.x / divisor[pixelQuality];
        pixelPerfectCam.refResolutionY = maxResolution.y / divisor[pixelQuality];
        pixelPerfectCam.assetsPPU = 60 / divisor[pixelQuality];
        currentResolution = new Vector2Int(1920 / divisor[pixelQuality], 1080 / divisor[pixelQuality]);
    }

    private void OnValidate()
    {
        SetPixelQuality();
    }
}
