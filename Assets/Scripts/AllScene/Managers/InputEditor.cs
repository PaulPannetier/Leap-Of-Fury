using UnityEngine;

public class InputEditor : MonoBehaviour
{
    public static InputEditor instance;

    [SerializeField] private bool enableListenKeyCode = false;

    #if UNITY_EDITOR

    [Header("Input Saver")]
    [SerializeField] private KeyboardKey[] inputsKeyForKeyboard;
    [SerializeField] private GamepadKey[] inputsKeyForGamepad;
    [SerializeField] private string[] inputsActions;
    [SerializeField] private bool saveInput = false;

    #endif


    private void OnValidate()
    {
        #if UNITY_EDITOR

        if (saveInput)
        {
            InputManager.ClearAll();
            if(inputsKeyForKeyboard != null && inputsActions != null && inputsKeyForKeyboard.Length == inputsActions.Length)
            {
                InputManager.AddInputActions(inputsActions, inputsKeyForKeyboard, BaseController.Keyboard, true);
            }

            if (inputsKeyForGamepad != null && inputsActions != null && inputsKeyForGamepad.Length == inputsActions.Length)
            {
                InputManager.AddInputActions(inputsActions, inputsKeyForGamepad, BaseController.Gamepad, true);
            }
            InputManager.SaveConfiguration(@"/Save/inputs" + SettingsManager.saveFileExtension);
        }

        #endif
    }


    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        InputManager.LoadConfiguration(@"/Save/inputs" + SettingsManager.saveFileExtension);
        EventManager.instance.callbackPreUpdate += InputManager.PreUpdate;
    }


    private void Update()
    {
        #if UNITY_EDITOR

        void ListenAndShowInput()
        {
            if (InputManager.Listen(ControllerType.All, out InputKey key))
            {
                Debug.Log("Input listen : " + InputManager.KeyToString(key));
            }
        }

        if (enableListenKeyCode)
            ListenAndShowInput();

        #endif

    }



}
