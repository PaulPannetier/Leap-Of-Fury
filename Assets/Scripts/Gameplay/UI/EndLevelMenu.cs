using System;
using UnityEngine;
using UnityEngine.UI;
using static EndMenuPlayerDisplayer;

public class EndLevelMenu : MonoBehaviour
{
    private EndMenuPlayerDisplayer[] displayers;

    [SerializeField] private VerticalLayoutGroup layoutGroup;
    [SerializeField] private GameObject playerMenuDisplayerPrefabs;
    [SerializeField] private float displayDuration = 3f;

    private void Start()
    {
        EventManager.instance.callbackOnLevelEnd += OnLevelEnd;
    }

    private void OnLevelEnd(LevelManager.EndLevelData endLevelData)
    {
        displayers = new EndMenuPlayerDisplayer[endLevelData.playersScore.Length];

        for (int i = 0; i < endLevelData.playersScore.Length; i++)
        {
            LevelManager.PlayerScore playerScore = endLevelData.playersScore[i];

            DisplaySettings displaySettings = new DisplaySettings(LevelManager.PlayerScore.nbKillsToWin, playerScore.nbKills, playerScore.playerCommon.charImageUIPrefabs, displayDuration);
            GameObject psDisplayer = Instantiate(playerMenuDisplayerPrefabs, layoutGroup.transform);
            displayers[i] = psDisplayer.GetComponent<EndMenuPlayerDisplayer>();
            displayers[i].Display(displaySettings);
        }

        Invoke(nameof(EndMenu), displayDuration);
    }

    private void EndMenu()
    {
        foreach (EndMenuPlayerDisplayer displayer in displayers)
        {
            displayer.EndDisplay();
        }

        displayers = Array.Empty<EndMenuPlayerDisplayer>();
        LevelManager.instance.OnEndDisplayEndMenu();
    }

    private void OnDestroy()
    {
        EventManager.instance.callbackOnLevelEnd -= OnLevelEnd;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        displayDuration = Mathf.Max(displayDuration, 0f);
    }

#endif
}
