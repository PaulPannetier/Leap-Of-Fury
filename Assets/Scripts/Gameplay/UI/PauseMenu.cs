using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    private bool isLevelPlaying;

    [SerializeField] private InputManager.GeneralInput pauseInput;
    [SerializeField] private Button resumeButton;
    [SerializeField] private TMP_Text resumeText;
    [SerializeField] private Button mapSelectionButton;
    [SerializeField] private TMP_Text mapSelectionText;
    [SerializeField] private Button mainTitleButton;
    [SerializeField] private TMP_Text mainTitleText;

    private void Start()
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }

        EventManager.instance.callbackOnLevelStart += OnLevelStart;
        EventManager.instance.callbackOnLevelRestart += OnLevelRestart;
        EventManager.instance.callbackOnLevelEnd += OnLevelEnd;
    }

    private void OnLevelStart(string levelName)
    {
        isLevelPlaying = true;
    }

    private void OnLevelRestart(string levelName)
    {
        isLevelPlaying = true;
    }

    private void OnLevelEnd(LevelManager.EndLevelData endLevelData)
    {
        isLevelPlaying = false;
    }

    private void Update()
    {
        if(isLevelPlaying && pauseInput.IsPressedDown())
        {
            if(PauseManager.instance.isPauseEnable)
            {
                OnPauseDisable();
            }
            else
            {
                OnPauseEnable();
            }
        }
    }

    private void OnResumeButtonDown()
    {
        OnPauseDisable();
    }

    private void OnMapSelectionButtonDown()
    {
        PauseManager.instance.DisablePause();
        SelectionMapOldSceneData selectionMapSceneData = TransitionManager.instance.GetOldSceneData("Selection Map") as SelectionMapOldSceneData;
        TransitionManager.instance.LoadSceneAsync("Selection Map", new LevelOldSceneData(TransitionManager.instance.activeScene, selectionMapSceneData.charData));
    }

    private void OnMainTitleButtonDown()
    {
        PauseManager.instance.DisablePause();
        TransitionManager.instance.LoadSceneAsync("Screen Title");
    }

    private void OnPauseEnable()
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(true);
        }

        resumeButton.onClick.RemoveAllListeners();
        mapSelectionButton.onClick.RemoveAllListeners();
        mainTitleButton.onClick.RemoveAllListeners();

        resumeButton.onClick.AddListener(OnResumeButtonDown);
        mapSelectionButton.onClick.AddListener(OnMapSelectionButtonDown);
        mainTitleButton.onClick.AddListener(OnMainTitleButtonDown);

        resumeText.text = LanguageManager.instance.GetText("resumeButton");
        mapSelectionText.text = LanguageManager.instance.GetText("mapSelectionButton");
        mainTitleText.text = LanguageManager.instance.GetText("mainTitleButton");

        PauseManager.instance.EnablePause();
    }

    private void OnPauseDisable()
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }
        PauseManager.instance.DisablePause();
    }

    private void OnDestroy()
    {
        EventManager.instance.callbackOnLevelStart -= OnLevelStart;
        EventManager.instance.callbackOnLevelRestart -= OnLevelRestart;
        EventManager.instance.callbackOnLevelEnd -= OnLevelEnd;
    }
}
