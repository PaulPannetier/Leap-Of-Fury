using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class ControlItem : MonoBehaviour
{
    private TextMeshProUGUI nameText, keyText;
    private Button keyButton;
    private BaseController controller => ControlManagerSettingMenu.instance.GetSelectedBaseController();

    private InputKey _key;
    public InputKey key
    {
        get => _key;
        set
        {
            _key = value;
            keyText.text = InputManager.KeyToString(key);
        }
    }
    public bool isListening { get; private set; }
    public SelectableUI selectableUI { get; private set; }

    private void Awake()
    {
        nameText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        keyText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        keyButton = transform.GetChild(1).GetComponent<Button>();
        selectableUI = GetComponent<SelectableUI>();
        keyButton.onClick.AddListener(OnKeyButtonDown);
    }

    public void SetNameText(string text)
    {
        nameText.text = text;
    }

    private void OnKeyButtonDown()
    {
        StartListening();
    }

    public void StartListening()
    {
        isListening = true;
        keyText.text = string.Empty;
    }

    public void StopListening()
    {
        isListening = false;
    }

    private void SetKey(InputKey key)
    {
        this.key = key == InputKey.Escape ? InputKey.None : key;
    }

    private void Update()
    {
        if(isListening)
        {
            if(InputManager.Listen(controller, out InputKey key))
            {
                SetKey(key);
                StopListening();
            }
        }
    }
}
