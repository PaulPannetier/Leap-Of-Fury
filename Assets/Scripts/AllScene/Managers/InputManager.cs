using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    [SerializeField] private bool enableListenKeyCode = false;

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
        if(CustomInput.Listen(ControllerType.All, out int key))
        {
            Debug.Log("Input listen : " + CustomInput.KeyToString(key));
        }
    }
}
