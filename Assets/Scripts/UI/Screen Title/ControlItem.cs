using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class ControlItem : MonoBehaviour
{
    private TextMeshProUGUI nameText, keyText;
    private Button keyButton;
    private bool isListening;
    private BaseController controller;

    public InputKey _key;
    public InputKey key
    {
        get => _key;
        set
        {
            _key = value;
            keyText.text = InputManager.KeyToString(key);
        }
    }

    private void Awake()
    {
        nameText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        keyText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        keyButton = transform.GetChild(1).GetComponent<Button>();
        keyButton.onClick.AddListener(OnKeyButtonDown);
    }

    public void SetNameText(string text)
    {
        nameText.text = text;
    }

    public void OnKeyButtonDown()
    {
        if(!ControlManagerSettingMenu.instance.isInputListening)
        {
            isListening = true;
            controller = ControlManagerSettingMenu.instance.GetSelectedBaseController();
        }
    }

    private void Update()
    {
        if(isListening)
        {
            if(InputManager.Listen(controller, out InputKey key))
            {
                this.key = key;
            }
        }
    }
}
