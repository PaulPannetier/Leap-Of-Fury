using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
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

        PauseManager.instance.callBackOnPauseEnable += OnPauseEnable;
        PauseManager.instance.callBackOnPauseDisable += OnPauseDisable;
    }

    private void OnResumeButtonDown()
    {
        PauseManager.instance.DisablePause();
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
    }

    private void OnPauseDisable()
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        PauseManager.instance.callBackOnPauseEnable -= OnPauseEnable;
        PauseManager.instance.callBackOnPauseDisable -= OnPauseDisable;
    }
}
