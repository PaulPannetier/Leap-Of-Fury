using UnityEngine;
using TMPro;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    private bool isLevelPlaying;

    [SerializeField] private InputManager.GeneralInput pauseInput;
    [SerializeField] private TMP_Text resumeText;
    [SerializeField] private TMP_Text mapSelectionText;
    [SerializeField] private TMP_Text mainTitleText;
    [SerializeField] private SelectableUIGroup selectableUIGroup;

    private void Start()
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }

        EventManager.instance.callbackOnLevelStart += OnLevelStart;
        EventManager.instance.callbackOnLevelRestart += OnLevelRestart;
        EventManager.instance.callbackOnLevelEnd += OnLevelEnd;
        EventManager.instance.callbackOnLevelFinish += OnLevelFinish;
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

    private void OnLevelFinish(LevelManager.FinishLevelData finishLevelData)
    {
        isLevelPlaying = false;
    }

    private void Update()
    {
        if(isLevelPlaying && pauseInput.IsPressedDown())
        {
            if(PauseManager.instance.isPauseEnable)
            {
                DisablePause();
            }
            else
            {
                EnablePause();
            }
        }
    }

    public void OnResumeButtonDown()
    {
        DisablePause();
    }

    public void OnMapSelectionButtonDown()
    {
        SelectionMapOldSceneData selectionMapSceneData = TransitionManager.instance.GetOldSceneData("Selection Map") as SelectionMapOldSceneData;
        TransitionManager.instance.LoadSceneAsync("Selection Map", new LevelOldSceneData(TransitionManager.instance.activeScene, selectionMapSceneData.charData));
    }

    public void OnMainTitleButtonDown()
    {
        TransitionManager.instance.LoadSceneAsync("TitleScreen");
    }

    public void EnablePause()
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(true);
        }

        selectableUIGroup.Init();
        PauseManager.instance.EnablePause();
        InputManager.ShowMouseCursor();
    }

    private void DisablePause()
    {
        selectableUIGroup.ResetToDefault();
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }
        PauseManager.instance.DisablePause();
#if !UNITY_EDITOR
        InputManager.HideMouseCursor();
#endif
    }

    private void OnDestroy()
    {
        EventManager.instance.callbackOnLevelStart -= OnLevelStart;
        EventManager.instance.callbackOnLevelRestart -= OnLevelRestart;
        EventManager.instance.callbackOnLevelEnd -= OnLevelEnd;
    }
}
