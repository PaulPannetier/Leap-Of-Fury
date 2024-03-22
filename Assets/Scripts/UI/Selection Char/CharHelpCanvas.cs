using System;
using UnityEngine;
using TMPro;
using UnityEngine.Video;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;

public class CharHelpCanvas : MonoBehaviour
{
    private int id;
    private Action<int> callbackCloseHelpCanvas;
    private VideoPlayer videoPlayer;
    private TextMeshProUGUI explicationText;
    private int selectedHelpIndex;
    private AttackVideoData selectedData => helpData[selectedHelpIndex];

    [SerializeField] private Vector2 turningSelectorOffset;
    [SerializeField] private InputManager.GeneralInput closeHelpCanvasInput;
    [SerializeField] private InputManager.GeneralInput nextHelpInput, previousHelpInput;

    [Header("Content")]
    [SerializeField] private AttackVideoData[] helpData;

    private void Awake()
    {
        videoPlayer = GetComponentInChildren<VideoPlayer>();
        explicationText = GetComponentInChildren<TextMeshProUGUI>();
        selectedHelpIndex = 0;
    }

    private void Start()
    {
        OnUpdateUI();
    }

    public void Launch(TurningSelector turningSelector, ControllerType controllerType, int id, Action<int> callbackCloseHelpCanvas)
    {
        closeHelpCanvasInput.controllerType = controllerType;
        nextHelpInput.controllerType = controllerType;
        previousHelpInput.controllerType = controllerType;
        transform.position = turningSelector.center + turningSelectorOffset;
        this.id = id;
        this.callbackCloseHelpCanvas = callbackCloseHelpCanvas;
    }

    private void Update()
    {
        if(closeHelpCanvasInput.IsPressedDown())
        {
            callbackCloseHelpCanvas.Invoke(id);
            Destroy(gameObject);
        }

        if(nextHelpInput.IsPressedDown())
        {
            selectedHelpIndex = (selectedHelpIndex + 1) % helpData.Length;
            OnUpdateUI();
        }

        if (previousHelpInput.IsPressedDown())
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
        explicationText.text = LanguageManager.instance.GetText(selectedData.descriptionKey);
    }

    #region Gizmos/OnValidate

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw((Vector2)transform.position - turningSelectorOffset, 50f);
    }

#endif

    #endregion

    #region Custom struct

    [Serializable]
    private struct AttackVideoData
    {
        public VideoClip video;
        public string descriptionKey;
        public TextMeshProUGUI descriptionText;

        public AttackVideoData(VideoClip video, string description, TextMeshProUGUI descriptionText)
        {
            this.video = video;
            this.descriptionKey = description;
            this.descriptionText = descriptionText;
        }
    }

    #endregion
}
