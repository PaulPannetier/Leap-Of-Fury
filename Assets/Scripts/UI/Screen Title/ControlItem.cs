using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class ControlItem : MonoBehaviour
{
    private static bool isAnInputListening;

    private TextMeshProUGUI nameText;
    private Image keyImage;
    private Button keyButton;
    private BaseController controller => ControlManagerSettingMenu.instance.GetSelectedBaseController();
    private bool isStartingListeningThisFrame;
    private InputKey _key;
    public InputKey key
    {
        get => _key;
        set
        {
            _key = value;
            if(controller == BaseController.Keyboard)
            {
                keyImage.sprite = InputIconManager.instance.GetButtonSprite(ControllerModel.Keyboard, value);
            }
            else
            {
                ControllerModel type = InputManager.GetCurrentGamepadModel();
                type = type == ControllerModel.None ? ControllerModel.XBoxSeries : type;
                keyImage.sprite = InputIconManager.instance.GetButtonSprite(type, value);
            }
        }
    }
    public bool isListening { get; private set; }
    public SelectableUI selectableUI { get; private set; }

    private void Awake()
    {
        nameText = GetComponentInChildren<TextMeshProUGUI>();
        keyImage = GetComponentInChildren<Image>();
        keyButton = GetComponentInChildren<Button>();
        selectableUI = GetComponent<SelectableUI>();
        keyButton.onClick.AddListener(OnKeyButtonDown);
    }

    public void SetNameText(string text)
    {
        nameText.text = text;
    }

    public void OnKeyButtonDown()
    {
        if(!isAnInputListening)
        {
            StartListening();
        }
    }

    private void StartListening()
    {
        isListening = isStartingListeningThisFrame = isAnInputListening = true;
        keyImage.sprite = InputIconManager.instance.unknowButton;
    }

    public void StopListening()
    {
        isListening = false;
        StopCoroutine(nameof(StopListeningCorout));
        StartCoroutine(StopListeningCorout());
    }

    private IEnumerator StopListeningCorout()
    {
        yield return null;
        yield return null;
        isAnInputListening = false;
    }

    private void SetKey(InputKey key)
    {
        this.key = key == InputKey.Escape ? InputKey.None : key;
    }

    private void Update()
    {
        if(isListening)
        {
            if(InputManager.ListenUp(controller, out InputKey key) && !isStartingListeningThisFrame)
            {
                SetKey(key);
                StopListening();
            }
            isStartingListeningThisFrame = false;
        }
    }
}
