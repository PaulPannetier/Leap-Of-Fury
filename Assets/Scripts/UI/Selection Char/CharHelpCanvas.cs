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
    [SerializeField] private CustomInput.GeneralInput closeHelpCanvasInput;
    [SerializeField] private CustomInput.GeneralInput nextHelpInput, previousHelpInput;

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

    #endregion
}
