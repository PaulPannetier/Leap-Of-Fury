using System;
using UnityEngine;
using TMPro;
using UnityEngine.Video;
using Collision2D;

public class CharHelpCanvas : MonoBehaviour
{
    private int id;
    private Action<int> callbackCloseHelpCanvas;
    private VideoPlayer videoPlayer;
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
        selectedHelpIndex = 0;
    }

    private void Start()
    {
        UpdateUI(selectedHelpIndex);
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
            Destroy(gameObject);
            callbackCloseHelpCanvas.Invoke(id);
            return;
        }

        if(nextHelpInput.IsPressedDown())
        {
            int newIndex = (selectedHelpIndex + 1) % helpData.Length;
            UpdateUI(newIndex);
        }

        if (previousHelpInput.IsPressedDown())
        {
            int newIndex = (selectedHelpIndex - 1);
            if (newIndex < 0)
                newIndex = helpData.Length - 1;
            UpdateUI(newIndex);
        }
    }

    private void UpdateUI(int newIndex)
    {
        selectedData.descriptionText.gameObject.SetActive(false);
        selectedHelpIndex = newIndex;
        videoPlayer.clip = selectedData.video;
        videoPlayer.Play();
        selectedData.descriptionText.gameObject.SetActive(true);
        selectedData.descriptionText.text = LanguageManager.instance.GetText(selectedData.descriptionKey).Resolve();
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
