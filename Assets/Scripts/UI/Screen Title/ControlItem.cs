using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class ControlItem : MonoBehaviour
{
    private static bool isAnInputListening;

    private Image keyImage;
    private bool isStartingListeningThisFrame;
    public InputKey keyboardKey { get; private set; }
    public InputKey gamepadKey { get; private set; }
    public InputKey key { get; private set; }
    public bool isListening { get; private set; }
    public SelectableUI selectableUI { get; private set; }

    [SerializeField] private Color disableColor;
    [SerializeField] private TextMeshProUGUI nameText;

    private bool _interactable;
    public bool interactable
    {
        get => _interactable;
        set
        {
            _interactable = value;
            keyImage.color = value ? Color.white : disableColor;
            selectableUI.interactable = value;
        }
    }

    [HideInInspector] public ControlManagerSettingMenu controlManagerSettingMenu;

    private void Awake()
    {
        keyImage = GetComponentInChildren<Image>();
        selectableUI = GetComponent<SelectableUI>();
    }

    public void Init(InputKey kbKey, InputKey gpKey)
    {
        keyboardKey = kbKey;
        gamepadKey = gpKey;
        SetCurrentKey(kbKey, BaseController.Keyboard);
    }

    public void SetNameText(string text)
    {
        nameText.text = text;
    }

    public void SetCurrentKey(InputKey key, BaseController baseController)
    {
        this.key = key;
        if (baseController == BaseController.Keyboard)
        {
            keyImage.sprite = InputIconManager.instance.GetButtonSprite(ControllerModel.Keyboard, key);
            keyboardKey = key;
        }
        else
        {
            ControllerModel type = InputManager.GetCurrentGamepadModel();
            type = type == ControllerModel.None ? ControllerModel.XBoxSeries : type;
            keyImage.sprite = InputIconManager.instance.GetButtonSprite(type, key);
            gamepadKey = key;
        }
    }

    public void OnKeyButtonDown()
    {
        if(interactable && !isAnInputListening)
        {
            StartListening();
        }
    }

    private void StartListening()
    {
        isListening = isStartingListeningThisFrame = isAnInputListening = true;
        keyImage.sprite = InputIconManager.instance.unknowButton;
        controlManagerSettingMenu.OnControlItemStartListening(this);
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
        controlManagerSettingMenu.OnControlItemStopListening(this);
    }

    private void Update()
    {
        if (!interactable)
            return;

        if(isListening)
        {
            BaseController currentController = controlManagerSettingMenu.GetSelectedBaseController();
            if (InputManager.ListenUp(currentController, out InputKey key) && !isStartingListeningThisFrame)
            {
                SetCurrentKey(key, currentController);
                StopListening();
            }
            isStartingListeningThisFrame = false;
        }
    }
}
