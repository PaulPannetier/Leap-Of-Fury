using UnityEngine;

public class InputIconManager : MonoBehaviour
{
    public static InputIconManager instance { get; private set; }

    [SerializeField] private UIIconsData iconsData;
    private Sprite unknowButton => iconsData.unknowButton;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    public Sprite GetButtonSprite(BaseController baseController, InputKey key)
    {
        if(baseController == BaseController.KeyboardAndGamepad)
        {
            string errorMsg = "Can't return the sprite button of KeyboardAndGamepad controller!";
            Debug.LogError(errorMsg);
            LogManager.instance.AddLog(errorMsg, "InputIconManager.GetButtonSprite(BaseController baseController)");
            return unknowButton;
        }

        InputControllerType controllerType = baseController == BaseController.Keyboard ? InputControllerType.Keyboard : InputControllerType.XBoxSeries;
        return GetButtonSprite(controllerType, key);
    }

    public Sprite GetKeyboardButtonSprite(KeyboardKey key)
    {
        return GetButtonSprite(InputControllerType.Keyboard, (InputKey)key);
    }

    public Sprite GetGamepadButtonSprite(GeneralGamepadKey key)
    {
        return GetButtonSprite(InputControllerType.XBoxSeries, (InputKey)key);
    }

    public Sprite GetGamepadButtonSprite(InputControllerType inputControllerType, GeneralGamepadKey key)
    {
        if(inputControllerType == InputControllerType.Keyboard)
        {
            string errorMsg = "InputControllerType.Keyboard is not a Gamepad controller!";
            Debug.LogError(errorMsg);
            LogManager.instance.AddLog(errorMsg, "InputIconManager.GetGamepadButtonSprite(InputControllerType inputControllerType, GeneralGamepadKey key)");
            return unknowButton;
        }

        return GetButtonSprite(inputControllerType, (InputKey)key);
    }

    public Sprite GetButtonSprite(InputControllerType inputControllerType, InputKey inputKey)
    {
        return iconsData.controllerData[inputControllerType].buttonsSprite[inputKey];
    }
}
