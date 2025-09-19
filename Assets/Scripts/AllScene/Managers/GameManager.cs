using UnityEngine;

using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private int[] divisor = new int[11] { 60, 30, 20, 15, 12, 10, 6, 5, 3, 2, 1 };
    private UnityEngine.Rendering.Universal.PixelPerfectCamera pixelPerfectCam;

    [SerializeField][Range(0, 10)] private int pixelQuality = 10;
    [SerializeField] private Vector2Int maxResolution = new Vector2Int(1920, 1080);
    public Vector2Int currentResolution = new Vector2Int(1920, 1080);

#if UNITY_EDITOR
    [SerializeField] private bool deterministicMode = true;
    [SerializeField] private int seed = 54134782;
#endif

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        pixelPerfectCam = Camera.main.GetComponent<UnityEngine.Rendering.Universal.PixelPerfectCamera>();
        transform.parent = null;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Application.quitting += OnQuitApplication;

#if UNITY_EDITOR
        if (deterministicMode)
            Random.SetSeed(seed);
        else
            Random.SetRandomSeed();
#else
        Random.SetRandomSeed();
#endif
        InputManager.ShowMouseCursor();
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

    private void OnQuitApplication()
    {
        //clear tmp files
        string path = Path.Combine(Application.dataPath, "Save", "GameData", "tmp");
        string[] dirs = Directory.GetDirectories(path);
        string[] files = Directory.GetFiles(path);

        foreach (string dir in dirs)
        {
            Directory.Delete(Path.Combine(path, dir), true);
        }
        foreach (string file in files)
        {
            File.Delete(Path.Combine(path, file));
        }
    }

    private void OnDestroy()
    {
        Application.quitting -= OnQuitApplication;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        SetPixelQuality();
    }

#endif
}
