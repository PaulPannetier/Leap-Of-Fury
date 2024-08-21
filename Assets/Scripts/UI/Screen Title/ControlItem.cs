using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class ControlItem : MonoBehaviour
{
    private TextMeshProUGUI nameText, keyText;
    private Button keyButton;
    private bool isListening;
    private BaseController controller;

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
        else
        {
            StopListening();
        }
    }

    public void StopListening()
    {
        isListening = false;
    }

    public void SetKey(InputKey key)
    {
        this.key = key == InputKey.Escape ? InputKey.None : key;
    }

    private void Update()
    {
        if(isListening)
        {
            if(InputManager.Listen(controller, out InputKey key))
            {
                if(ControlManagerSettingMenu.instance.GetSelectedBaseController() == BaseController.Gamepad)
                {
                    if(InputManager.IsGamepadKey(key))
                        SetKey(key);
                }
                else
                {
                    if (InputManager.IsKeyboardKey(key))
                        SetKey(key);
                }
                StopListening();
            }
        }
    }
}
