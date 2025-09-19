using UnityEngine;

[RequireComponent (typeof(LineRenderer))]
public class LineRendererAnimator : MonoBehaviour
{
    private float counter;
    private int textureIndex;
    private LineRenderer lineRenderer;

    [SerializeField] private bool startOnAwake = true;
    [SerializeField] private Texture[] textures;
    [SerializeField] private string textureToChange = "_MainTex";
    [SerializeField] private float animationFPS;

    [HideInInspector] public bool enableBehaviour;

    private void Awake()
    {
        enableBehaviour = startOnAwake;
        counter = 0f;
        textureIndex = 0;
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        UpdateLineRendererVisual();
    }

    private void UpdateLineRendererVisual()
    {
        textureIndex = (textureIndex + 1) % textures.Length;
        lineRenderer.material.SetTexture(textureToChange, textures[textureIndex]);
    }

    private void Update()
    {
        if(!enableBehaviour || PauseManager.instance.isPauseEnable)
            return;

        counter += Time.deltaTime;
        if(counter > 1f / animationFPS)
        {
            UpdateLineRendererVisual();
            counter -= 1f / animationFPS;
        }
    }

    #region OnValidate

#if UNITY_EDITOR

    private void OnValidate()
    {
        lineRenderer = GetComponent<LineRenderer>();
        animationFPS = Mathf.Max(0f, animationFPS);
        if(textures != null && textures.Length > 0 && lineRenderer.sharedMaterial != null)
        {
            lineRenderer.sharedMaterial.SetTexture(textureToChange, textures[0]);
        }
    }

#endif

    #endregion
}
