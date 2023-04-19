using System;
using UnityEngine;
using TMPro;
using UnityEngine.Video;

public class CharHelpCanvas : MonoBehaviour
{
    private TurningSelector turningSelector;
    private ControllerType controllerType;
    private int id;
    private Action<int> callbackCloseHelpCanvas;
    private VideoPlayer videoPlayer;
    private TextMeshProUGUI explicationText;
    private int selectedHelpIndex;
    private AttackVideoData selectedData => helpData[selectedHelpIndex];

    [SerializeField] private Vector2 turningSelectorOffset;
    [SerializeField] private GeneralInput closeHelpCanvasInput;
    [SerializeField] private GeneralInput nextHelpInput, previousHelpInput;

    [Header("Content")]
    [SerializeField] private AttackVideoData[] helpData;

    private void Awake()
    {
        videoPlayer = GetComponentInChildren<VideoPlayer>();
        explicationText = GetComponentInChildren<TextMeshProUGUI>();  
    }

    private void Start()
    {
        selectedHelpIndex = 0;
        OnUpdateUI();
    }

    public void Lauch(TurningSelector turningSelector, ControllerType controllerType, int id, Action<int> callbackCloseHelpCanvas)
    {
        this.turningSelector = turningSelector;
        this.controllerType = controllerType;
        closeHelpCanvasInput.controllerType = controllerType;
        transform.position = turningSelector.center + turningSelectorOffset;
        this.id = id;
        this.callbackCloseHelpCanvas = callbackCloseHelpCanvas;
    }

    private void Update()
    {
        if(closeHelpCanvasInput.IsPresseDown())
        {
            callbackCloseHelpCanvas.Invoke(id);
            Destroy(gameObject);
        }

        if(nextHelpInput.IsPresseDown())
        {
            selectedHelpIndex = (selectedHelpIndex + 1) % helpData.Length;
            OnUpdateUI();
        }

        if (previousHelpInput.IsPresseDown())
        {
            selectedHelpIndex = (selectedHelpIndex - 1);
            if (selectedHelpIndex < 0)
                selectedHelpIndex = helpData.Length - 1;
            OnUpdateUI();
        }
    }

    private void OnUpdateUI()
    {
        videoPlayer.clip = selectedData.video;
        videoPlayer.Play();
        explicationText.text = selectedData.description;
    }

    #region Gizmos/OnValidate

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw((Vector2)transform.position - turningSelectorOffset, 50f);
    }

    #endregion

    #region Custom struct

    [Serializable]
    private struct AttackVideoData
    {
        public VideoClip video;
        public string description;

        public AttackVideoData(VideoClip video, string description)
        {
            this.video = video;
            this.description = description;
        }
    }

    [Serializable]
    private struct GeneralInput
    {
        public KeyCode[] keysKeyboard, keyGamepad1, keyGamepad2, keyGamepad3, keyGamepad4;
        public ControllerType controllerType;

        public GeneralInput(KeyCode[] keysKeyboard, KeyCode[] keyGamepad1, KeyCode[] keyGamepad2, KeyCode[] keyGamepad3, KeyCode[] keyGamepad4, ControllerType controllerType)
        {
            this.keysKeyboard = keysKeyboard;
            this.keyGamepad1 = keyGamepad1;
            this.keyGamepad2 = keyGamepad2;
            this.keyGamepad3 = keyGamepad3;
            this.keyGamepad4 = keyGamepad4;
            this.controllerType = controllerType;
        }

        private bool isKeySomething(Func<KeyCode, bool> func)
        {
            switch (controllerType)
            {
                case ControllerType.Keyboard:
                    return GetKeySomething(func, keysKeyboard);
                case ControllerType.Gamepad1:
                    return GetKeySomething(func, keyGamepad1);
                case ControllerType.Gamepad2:
                    return GetKeySomething(func, keyGamepad2);
                case ControllerType.Gamepad3:
                    return GetKeySomething(func, keyGamepad3);
                case ControllerType.Gamepad4:
                    return GetKeySomething(func, keyGamepad4);
                case ControllerType.GamepadAll:
                    return GetKeySomething(func, keyGamepad1) || GetKeySomething(func, keyGamepad2)
                        || GetKeySomething(func, keyGamepad3) || GetKeySomething(func, keyGamepad4);
                case ControllerType.All:
                    return GetKeySomething(func, keysKeyboard) || GetKeySomething(func, keyGamepad1) || GetKeySomething(func, keyGamepad2)
                        || GetKeySomething(func, keyGamepad3) || GetKeySomething(func, keyGamepad4);
                default:
                    return false;
            }

            bool GetKeySomething(Func<KeyCode, bool> func, KeyCode[] keyCodes)
            {
                foreach (KeyCode key in keyCodes)
                {
                    if (func(key))
                        return true;
                }
                return false;
            }
        }

        public bool IsPresseDown() => isKeySomething((KeyCode key) => CustomInput.GetKeyDown(key));
        public bool IsPressedUp() => isKeySomething((KeyCode key) => CustomInput.GetKeyUp(key));
        public bool IsPressed() => isKeySomething((KeyCode key) => CustomInput.GetKey(key));
    }

    #endregion
}
