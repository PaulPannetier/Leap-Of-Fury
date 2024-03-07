using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class DebugText : MonoBehaviour
{
    public static DebugText instance;

    [SerializeField] private TextMeshProUGUI _Text;
    public string text
    {
        get => _Text.text;
        set
        {
            _Text.text = value;
        }
    }

    public float size
    {
        get => _Text.fontSize;
        set
        {
            _Text.fontSize = value;
        }
    }

    private void Awake()
    {

#if !UNITY_EDITOR

        Destroy(gameObject);
        return;

#else

        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        _Text = GetComponent<TextMeshProUGUI>();

#endif
    }

    private void Start()
    {
        EventManager.instance.callbackPreUpdate += PreUpdate;
    }

    private void PreUpdate()
    {
        text = "";
    }
}
