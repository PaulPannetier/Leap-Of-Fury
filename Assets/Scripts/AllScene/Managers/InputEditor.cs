using System;
using UnityEngine;

public class InputEditor : MonoBehaviour
{
    public static InputEditor instance;

    private const string inputPath = @"/Save/UserSave/inputs" + SettingsManager.saveFileExtension;

#if UNITY_EDITOR

    [SerializeField] private bool enableListenKeyCode = false;
    [SerializeField] private bool printControllerModel = false;

#endif

    [Header("Input Saver")]
    [SerializeField] private InputDataKB[] inputsKeyForKeyboard;
    [SerializeField] private InputDataGP[] inputsKeyForGamepad;
    [SerializeField] private string[] inputsActions;

#if UNITY_EDITOR

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

        if(!InputManager.LoadConfiguration(inputPath))
        {
            SaveDefaultInputConfig();
        }
    }

    public void SaveInputConfig()
    {
        InputManager.SaveConfigurationAsync(inputPath,(b) => { }).GetAwaiter();
    }

    private void SaveDefaultInputConfig()
    {
        InputManager.ClearAll();
        if (inputsKeyForKeyboard != null && inputsActions != null && inputsKeyForKeyboard.Length == inputsActions.Length)
        {
            for (int i = 0; i < inputsActions.Length; i++)
            {
                InputManager.AddInputsAction(inputsActions[i], inputsKeyForKeyboard[i].keys, BaseController.Keyboard, true);
                InputManager.AddInputsAction(inputsActions[i], inputsKeyForKeyboard[i].keys, BaseController.Keyboard, false);
            }
        }

        if (inputsKeyForGamepad != null && inputsActions != null && inputsKeyForGamepad.Length == inputsActions.Length)
        {
            for (int i = 0; i < inputsActions.Length; i++)
            {
                InputManager.AddInputsAction(inputsActions[i], inputsKeyForGamepad[i].keys, BaseController.Gamepad, true);
                InputManager.AddInputsAction(inputsActions[i], inputsKeyForGamepad[i].keys, BaseController.Gamepad, false);
            }
        }
        InputManager.SaveConfigurationAsync(inputPath, (b) => { }).GetAwaiter();
    }

#if UNITY_EDITOR

    private void Update()
    {
        void ListenAndShowInput()
        {
            if (InputManager.Listen(ControllerType.Any, out InputKey key))
            {
                Debug.Log("Input listen : " + InputManager.KeyToString(key));
            }
        }

        if (enableListenKeyCode)
            ListenAndShowInput();
    }

    private void OnValidate()
    {
        if (saveInput)
        {
            saveInput = false;
            SaveDefaultInputConfig();
        }

        if(printControllerModel)
        {
            printControllerModel = false;
            print(InputManager.GetControllerModel(ControllerType.Gamepad1));
        }
    }

#endif

    #region Custom Struct

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

    #endregion
}
