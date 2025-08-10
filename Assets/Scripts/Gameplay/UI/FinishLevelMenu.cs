using UnityEngine;

public class FinishLevelMenu : MonoBehaviour
{
    private bool isMenuDisplayed;
    private float lastTimeOpenFinishMenu;

    [SerializeField] private GameObject finishMenuPlayerDisplayerPrefab;
    [SerializeField] private Transform layoutGroup;
    [SerializeField] private InputManager.GeneralInput echapInput;
    [SerializeField] private float minDurationInMenu;

    private void Start()
    {
        EventManager.instance.callbackOnLevelFinish += OnLevelFinish;
    }

    private void OnLevelFinish(LevelManager.FinishLevelData finishLevelData)
    {
        PauseManager.instance.EnablePause();

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }

        foreach (LevelManager.PlayerScore score in finishLevelData.playersScore)
        {
            GameObject playerDisplayer = Instantiate(finishMenuPlayerDisplayerPrefab, layoutGroup);
            FinishMenuPlayerDisplayer finishMenuPlayer = playerDisplayer.GetComponent<FinishMenuPlayerDisplayer>();
            finishMenuPlayer.Init(score, score.nbKills >= LevelManager.PlayerScore.nbKillsToWin);
        }

        isMenuDisplayed = true;
        lastTimeOpenFinishMenu = Time.time;
    }

    private void Update()
    {
        if (!isMenuDisplayed)
            return;

        if (Time.time - lastTimeOpenFinishMenu >= minDurationInMenu && echapInput.IsPressedDown())
        {
            CloseMenu();
        }
    }

    private void CloseMenu()
    {
        foreach (Transform child in layoutGroup.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        PauseManager.instance.DisablePause();

        SelectionMapOldSceneData selectionMapSceneData = (SelectionMapOldSceneData)TransitionManager.instance.GetOldSceneData("Selection Map");
        TransitionManager.instance.LoadSceneAsync("Selection Map", new LevelOldSceneData(TransitionManager.instance.activeScene, selectionMapSceneData.charData));
    }

    private void OnDestroy()
    {
        EventManager.instance.callbackOnLevelFinish -= OnLevelFinish;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        minDurationInMenu = Mathf.Max(0f, minDurationInMenu);
    }

#endif
}
