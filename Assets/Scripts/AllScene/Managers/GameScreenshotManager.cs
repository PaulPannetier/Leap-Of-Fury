using UnityEngine;

public class GameScreenshotManager : MonoBehaviour
{
    public static GameScreenshotManager instance;

    [SerializeField] private Texture cameraTextureScreenshot;
    [SerializeField] private Texture screenshot;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public Texture DoGameScreenshot()
    {        
        Graphics.CopyTexture(cameraTextureScreenshot, screenshot);
        return screenshot;
    }

    public Texture DoScreenshot()
    {
        float factor = Screen.currentResolution.width / 1920f;
        //ScreenCapture.CaptureScreenshot(, factor);
        return null;
    }
}
