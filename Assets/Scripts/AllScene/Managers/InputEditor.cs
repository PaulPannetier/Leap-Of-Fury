using System;
using UnityEngine;

public class InputEditor : MonoBehaviour
{
    public static InputEditor instance;

    [SerializeField] private bool enableListenKeyCode = false;

    #if UNITY_EDITOR

    [Header("Input Saver")]
    [SerializeField] private InputDataKB[] inputsKeyForKeyboard;
    [SerializeField] private InputDataGP[] inputsKeyForGamepad;
    [SerializeField] private string[] inputsActions;
    [SerializeField] private bool saveInput = false;

    #endif

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


    private void OnValidate()
    {
        #if UNITY_EDITOR

        if (saveInput)
        {
            InputManager.ClearAll();
            if (inputsKeyForKeyboard != null && inputsActions != null && inputsKeyForKeyboard.Length == inputsActions.Length)
            {
                for (int i = 0; i < inputsActions.Length; i++)
                {
                    InputManager.AddInputsAction(inputsActions[i], inputsKeyForKeyboard[i].keys, BaseController.Keyboard, true);
                }
            }

            if (inputsKeyForGamepad != null && inputsActions != null && inputsKeyForGamepad.Length == inputsActions.Length)
            {
                for (int i = 0; i < inputsActions.Length; i++)
                {
                    InputManager.AddInputsAction(inputsActions[i], inputsKeyForGamepad[i].keys, BaseController.Gamepad, true);
                }
            }
            InputManager.SaveConfiguration(@"/Save/inputs" + SettingsManager.saveFileExtension);
            saveInput = false;
        }

        #endif
    }

#if UNITY_EDITOR

    [Serializable]
    private struct InputDataKB
    {
        public KeyboardKey[] keys;
    }

    [Serializable]
    private struct InputDataGP
    {
        public GeneralGamepadKey[] keys;
    }

#endif
}
