using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class CameraRecording : MonoBehaviour
{
    [SerializeField] private float recordingTime = 5f;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private Canvas recordCanvas;
    [SerializeField] private Material recordMat;

    private bool isRenderCam;
    private List<FrameData> images = new List<FrameData>();

    private void Start()
    {
        if (isRenderCam)
            return;

        GameObject renderCam = Instantiate(gameObject, transform.position, transform.rotation);
        renderCam.name = "Render Camera";
        renderCam.GetComponent<Camera>().targetTexture = renderTexture;
        renderCam.GetComponent<CameraRecording>().isRenderCam = true;
        Destroy(renderCam.GetComponent<AudioListener>());
        recordCanvas.enabled = false;

        int w = GameManager.instance.currentResolution.x;
        int h = GameManager.instance.currentResolution.y;
        renderTexture = new RenderTexture(w, h, 0);

        Destroy(this);
    }

    public void PlayRecording()
    {
        Image img = recordCanvas.GetComponentInChildren<Image>();
        StartCoroutine(DisplayRecording(img));
    }

    private IEnumerator DisplayRecording(Image image)
    {
        recordCanvas.enabled = true;
        Debug.Log("Activation du canvas");
        int count = 0;
        while(count < images.Count)
        {
            //Texture2D texture = images[count].img.ToTexture2D();
            //image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            yield return null;
            count++;
        }
        Debug.Log("Deactivation du canvas");
        recordCanvas.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            PlayRecording();

        images.Add(new FrameData(renderTexture, Time.time));

        while (true)
        {
            if (Time.time - recordingTime > images[0].time)
            {
                //images[0].img.Release();
                images.RemoveAt(0);
            }
            else
                break;
        }
    }

    [System.Serializable]
    private struct FrameData
    {
        public RenderTexture img;
        public float time;

        public FrameData(RenderTexture img, in float time)
        {
            this.img = img;
            this.time = time;
        }
    }
}
