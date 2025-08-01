using UnityEngine;

public class InputIconManager : MonoBehaviour
{
    public static InputIconManager instance { get; private set; }

    [SerializeField] private UIIconsData iconsData;
    public Sprite unknowButton => iconsData.unknowButton;

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
            string errorMsg = "Can't return the sprite button of Keyboard AND Gamepad controller!";
            Debug.LogError(errorMsg);
            LogManager.instance.AddLog(errorMsg);
            return unknowButton;
        }

        ControllerModel controllerType = baseController == BaseController.Keyboard ? ControllerModel.Keyboard : ControllerModel.XBoxSeries;
        return GetButtonSprite(controllerType, key);
    }

    public Sprite GetKeyboardButtonSprite(KeyboardKey key)
    {
        return GetButtonSprite(ControllerModel.Keyboard, (InputKey)key);
    }

    public Sprite GetGamepadButtonSprite(GeneralGamepadKey key)
    {
        return GetButtonSprite(ControllerModel.XBoxSeries, (InputKey)key);
    }

    public Sprite GetGamepadButtonSprite(ControllerModel inputControllerType, GeneralGamepadKey key)
    {
        if(inputControllerType == ControllerModel.Keyboard)
        {
            string errorMsg = "InputControllerType.Keyboard is not a Gamepad controller!";
            Debug.LogError(errorMsg);
            LogManager.instance.AddLog(errorMsg);
            return unknowButton;
        }

        return GetButtonSprite(inputControllerType, (InputKey)key);
    }

    public Sprite GetButtonSprite(ControllerModel inputControllerType, InputKey inputKey)
    {
        return iconsData.controllerData[inputControllerType].buttonsSprite[inputKey];
    }
}
