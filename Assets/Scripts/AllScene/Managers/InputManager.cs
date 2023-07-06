using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    [SerializeField] private bool enableListenKeyCode = false;

    [Header("Input Saver")]
    [SerializeField] private InputKey[] inputsKeyForKeyboard;
    [SerializeField] private string[] inputsKey;
    [SerializeField] private bool saveInput = false;

    private void OnValidate()
    {
        if (saveInput && inputsKeyCode.Length == inputsKey.Length)
        {
            CustomInput.LoadConfiguration(@"/Save/inputs" + SettingsManager.saveFileExtension);
            CustomInput.ClearPlayerConfiguration(playerIndexToSave, true);
            for (int i = 0; i < inputsKey.Length; i++)
            {
                CustomInput.AddInputAction(inputsKey[i], inputsKeyCode[i], playerIndexToSave);
            }
            CustomInput.SetDefaultControler();
            CustomInput.SaveConfiguration(@"/Save/inputs" + SettingsManager.saveFileExtension);
        }
        saveInput = false;
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
        CustomInput.LoadConfiguration(@"/Save/inputs" + SettingsManager.saveFileExtension);
        EventManager.instance.callbackPreUpdate += CustomInput.PreUpdate;
    }

    private void Update()
    {
        if (enableListenKeyCode)
            ListenAndShowInput();
    }

    private void ListenAndShowInput()
    {
        if(CustomInput.Listen(ControllerType.All, out InputKey key))
        {
            Debug.Log("Input listen : " + CustomInput.KeyToString(key));
        }
    }
}
